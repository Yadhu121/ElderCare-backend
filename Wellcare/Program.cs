//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Tokens;
//using System.Collections.Concurrent;
//using System.Data.SqlClient;
//using System.Net.WebSockets;
//using System.Text;
//using System.Text.Json;
//using wellcare.Models;
//using wellcare.Services;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllersWithViews();

//builder.Services.AddSingleton<DBConnect>();
//builder.Services.AddScoped<caretakerTable>();
//builder.Services.AddScoped<OtpTable>();
//builder.Services.AddSingleton<EmailService>();
//builder.Services.AddScoped<elderTable>();
//builder.Services.AddScoped<CaretakerElderService>();
//builder.Services.AddScoped<elderProfile>();
//builder.Services.AddScoped<JwtService>();
//builder.Services.AddScoped<PrescriptionTable>();
//builder.Services.AddSingleton<FcmService>();

//var jwtSettings = builder.Configuration.GetSection("Jwt");
//var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.Events = new JwtBearerEvents
//    {
//        OnMessageReceived = context =>
//        {
//            context.Token = context.Request.Cookies["access_token"];
//            return Task.CompletedTask;
//        }
//    };

//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,

//        ValidIssuer = jwtSettings["Issuer"],
//        ValidAudience = jwtSettings["Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(key)
//    };
//});

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll",
//        policy =>
//        {
//            policy
//                .AllowAnyOrigin()
//                .AllowAnyMethod()
//                .AllowAnyHeader();
//        });
//});

//var app = builder.Build();

//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//}

//app.UseStaticFiles();

//app.UseWebSockets();

//app.UseRouting();
//app.UseCors("AllowAll");

//app.UseAuthentication();
//app.UseAuthorization();

//var liveLocations = new ConcurrentDictionary<int, (double lat, double lon)>();

//var videoClients = new ConcurrentDictionary<int, List<WebSocket>>();
//var latestFrames = new ConcurrentDictionary<int, byte[]>();
//var pendingResponses = new ConcurrentDictionary<int, TaskCompletionSource<string>>();
//var elderSockets = new ConcurrentDictionary<int, WebSocket>();

//app.Map("/ws/location", async context =>
//{
//    if (!context.WebSockets.IsWebSocketRequest)
//    {
//        context.Response.StatusCode = 400;
//        return;
//    }

//    using var ws = await context.WebSockets.AcceptWebSocketAsync();
//    var buffer = new byte[4096];

//    while (true)
//    {
//        var result = await ws.ReceiveAsync(buffer, CancellationToken.None);

//        if (result.MessageType == WebSocketMessageType.Close)
//            break;

//        var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);

//        var data = JsonSerializer.Deserialize<LocationMsg>(msg);

//        if (data != null)
//        {
//            liveLocations[data.elderId] = (data.lat, data.lon);
//            Console.WriteLine($"Elder {data.elderId}: {data.lat}, {data.lon}");
//        }
//    }
//});

//app.MapGet("/api/live-locations", () =>
//{
//    return liveLocations.Select(x => new
//    {
//        elderId = x.Key,
//        lat = x.Value.lat,
//        lon = x.Value.lon
//    });
//}).RequireAuthorization();

//app.Map("/ws/video", async context =>
//{
//    if (!context.WebSockets.IsWebSocketRequest)
//    {
//        context.Response.StatusCode = 400;
//        return;
//    }

//    if (!context.Request.Query.TryGetValue("elderId", out var elderIdStr)
//        || !int.TryParse(elderIdStr, out int connectedElderId))
//    {
//        context.Response.StatusCode = 400;
//        return;
//    }

//    using var ws = await context.WebSockets.AcceptWebSocketAsync();

//    elderSockets[connectedElderId] = ws;
//    Console.WriteLine($"Elder {connectedElderId} connected to video WebSocket");

//    //var startCmd = JsonSerializer.SerializeToUtf8Bytes(new { command = "START_STREAM" });
//    //await ws.SendAsync(startCmd, WebSocketMessageType.Text, true, CancellationToken.None);

//    var buffer = new byte[1024 * 1024];
//    //int connectedElderId = -1;
//    //if (context.Request.Query.TryGetValue("elderId", out var eId))
//    //    connectedElderId = int.Parse(eId);

//    //elderSockets[connectedElderId] = ws;

//    while (true)
//    {
//        var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
//        if (result.MessageType == WebSocketMessageType.Close)
//            break;

//        var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
//        var data = JsonSerializer.Deserialize<JsonElement>(msg);

//        string eventType = data.GetProperty("event").GetString();
//        int elderId = data.GetProperty("elder_id").GetInt32();

//        if (eventType == "STREAM_FRAME" && data.TryGetProperty("image", out var imgProp))
//        {
//            var frameBytes = Convert.FromBase64String(imgProp.GetString());
//            latestFrames[elderId] = frameBytes;
//        }
//        else if (eventType == "FALL_DETECTED" || eventType == "IDLE_DETECTED")
//        {
//            Console.WriteLine($"Alert: {eventType} for Elder {elderId}");

//            byte[] snapshot = null;
//            if (data.TryGetProperty("image", out var snapProp))
//                snapshot = Convert.FromBase64String(snapProp.GetString());

//            _ = Task.Run(async () =>
//            {
//                try
//                {
//                    var db = app.Services.GetRequiredService<DBConnect>();
//                    var emailService = app.Services.GetRequiredService<EmailService>();
//                    var fcmService = app.Services.GetRequiredService<FcmService>();

//                    using var con = db.GetConnection();
//                    using var cmd = new SqlCommand(@"
//                        SELECT c.Email, c.FirstName, e.elderName, e.FCMToken
//                        FROM CaretakerElderMap m
//                        JOIN caretakerTable c ON c.CareTakerID = m.CareTakerID
//                        JOIN elderTable e ON e.elderId = m.ElderID
//                        WHERE m.ElderID = @elderId", con);

//                    cmd.Parameters.AddWithValue("@elderId", elderId);
//                    con.Open();

//                    using var reader = cmd.ExecuteReader();
//                    if (!reader.Read()) return;

//                    string caretakerEmail = reader["Email"].ToString();
//                    string elderName = reader["elderName"].ToString();
//                    string fcmToken = reader["FCMToken"]?.ToString();
//                    reader.Close();
//                    con.Close();

//                    await emailService.SendAlertEmailAsync(caretakerEmail, elderName, eventType, snapshot);
//                    Console.WriteLine($"Alert email sent to {caretakerEmail}");

//                    if (!string.IsNullOrEmpty(fcmToken))
//                    {
//                        string notifTitle = eventType == "FALL_DETECTED" ? "Fall Detected!" : "Idle Alert!";
//                        string notifBody = "Are you okay? Please respond within 60 seconds.";

//                        await fcmService.SendNotificationAsync(fcmToken, notifTitle, notifBody, new Dictionary<string, string>
//                        {
//                            { "elderId", elderId.ToString() },
//                            { "eventType", eventType }
//                        });

//                        var tcs = new TaskCompletionSource<string>();
//                        pendingResponses[elderId] = tcs;

//                        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(60));
//                        var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

//                        pendingResponses.TryRemove(elderId, out _);

//                        if (completedTask == timeoutTask)
//                        {
//                            await emailService.SendFollowUpEmailAsync(caretakerEmail, elderName, "NO_RESPONSE");
//                            Console.WriteLine("No response from elder - follow up email sent");
//                        }
//                        else
//                        {
//                            string response = tcs.Task.Result;
//                            if (response.ToLower() == "yes")
//                            {
//                                await emailService.SendFollowUpEmailAsync(caretakerEmail, elderName, "OKAY");
//                                Console.WriteLine("Elder is okay - follow up email sent");
//                            }
//                            else
//                            {
//                                await emailService.SendFollowUpEmailAsync(caretakerEmail, elderName, "NOT_OKAY");
//                                Console.WriteLine("Elder not okay - follow up email sent");
//                            }
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"Failed to send alert email: {ex.Message}");
//                }
//            });
//        }
//    }
//    elderSockets.TryRemove(connectedElderId, out _);
//    latestFrames.TryRemove(connectedElderId, out _);
//    Console.WriteLine($"Elder {connectedElderId} disconnected from video WebSocket");
//});

//app.MapGet("/stream/{elderId}", async (int elderId, HttpContext context) =>
//{
//    context.Response.ContentType = "multipart/x-mixed-replace; boundary=frame";

//    while (!context.RequestAborted.IsCancellationRequested)
//    {
//        if (latestFrames.TryGetValue(elderId, out var frame))
//        {
//            var header = "--frame\r\nContent-Type: image/jpeg\r\n\r\n";
//            var headerBytes = Encoding.UTF8.GetBytes(header);
//            var footer = Encoding.UTF8.GetBytes("\r\n");

//            await context.Response.Body.WriteAsync(headerBytes);
//            await context.Response.Body.WriteAsync(frame);
//            await context.Response.Body.WriteAsync(footer);
//            await context.Response.Body.FlushAsync();
//        }
//        await Task.Delay(33);
//    }
//});

//app.MapPost("/api/elder/respond", async (HttpContext context) =>
//{
//    var body = await JsonSerializer.DeserializeAsync<JsonElement>(context.Request.Body);
//    int elderId = body.GetProperty("elderId").GetInt32();
//    string response = body.GetProperty("response").GetString();

//    if (pendingResponses.TryGetValue(elderId, out var tcs))
//    {
//        tcs.TrySetResult(response);
//    }

//    return Results.Ok();
//});

//app.MapPost("/api/elder/start-stream/{elderId}", async (int elderId) =>
//{
//    if (elderSockets.TryGetValue(elderId, out var ws) && ws.State == WebSocketState.Open)
//    {
//        var cmd = JsonSerializer.SerializeToUtf8Bytes(new { command = "START_STREAM" });
//        await ws.SendAsync(cmd, WebSocketMessageType.Text, true, CancellationToken.None);
//        Console.WriteLine($"START_STREAM sent to Elder {elderId}");
//        return Results.Ok();
//    }
//    return Results.NotFound("Elder not connected");
//}).RequireAuthorization();

//app.MapPost("/api/elder/stop-stream/{elderId}", async (int elderId) =>
//{
//    if (elderSockets.TryGetValue(elderId, out var ws) && ws.State == WebSocketState.Open)
//    {
//        var cmd = JsonSerializer.SerializeToUtf8Bytes(new { command = "STOP_STREAM" });
//        await ws.SendAsync(cmd, WebSocketMessageType.Text, true, CancellationToken.None);
//        Console.WriteLine($"STOP_STREAM sent to Elder {elderId}");
//        return Results.Ok();
//    }
//    return Results.NotFound("Elder not connected");
//}).RequireAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=landing}/{action=landing}/{id?}");

//app.Run();


using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using wellcare.Models;
using wellcare.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<DBConnect>();
builder.Services.AddScoped<caretakerTable>();
builder.Services.AddScoped<OtpTable>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddScoped<elderTable>();
builder.Services.AddScoped<CaretakerElderService>();
builder.Services.AddScoped<elderProfile>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PrescriptionTable>();
builder.Services.AddSingleton<FcmService>();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["access_token"];
            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseWebSockets();

app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

var liveLocations = new ConcurrentDictionary<int, (double lat, double lon)>();

var videoClients = new ConcurrentDictionary<int, List<WebSocket>>();
var latestFrames = new ConcurrentDictionary<int, byte[]>();
var pendingResponses = new ConcurrentDictionary<int, TaskCompletionSource<string>>();
var elderSockets = new ConcurrentDictionary<int, WebSocket>();
var geofenceState = new ConcurrentDictionary<int, bool>();

app.Map("/ws/location", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    using var ws = await context.WebSockets.AcceptWebSocketAsync();
    var buffer = new byte[4096];

    while (true)
    {
        var result = await ws.ReceiveAsync(buffer, CancellationToken.None);

        if (result.MessageType == WebSocketMessageType.Close)
            break;

        var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);

        var data = JsonSerializer.Deserialize<LocationMsg>(msg);

        if (data != null)
        {
            liveLocations[data.elderId] = (data.lat, data.lon);
            Console.WriteLine($"Elder {data.elderId}: {data.lat}, {data.lon}");

            _ = Task.Run(async () =>
            {
                try
                {
                    var db = app.Services.GetRequiredService<DBConnect>();
                    var emailService = app.Services.GetRequiredService<EmailService>();

                    using var con = db.GetConnection();
                    using var cmd = new SqlCommand(@"
                        SELECT e.HomeLat, e.HomeLng, e.elderName, c.Email
                        FROM elderTable e
                        JOIN CaretakerElderMap m ON m.ElderID = e.elderId
                        JOIN caretakerTable c ON c.CareTakerID = m.CareTakerID
                        WHERE e.elderId = @elderId
                        AND e.HomeLat IS NOT NULL AND e.HomeLng IS NOT NULL", con);

                    cmd.Parameters.AddWithValue("@elderId", data.elderId);
                    con.Open();
                    using var reader = cmd.ExecuteReader();
                    if (!reader.Read()) return;

                    double homeLat = reader.GetDouble(0);
                    double homeLng = reader.GetDouble(1);
                    string elderName = reader.GetString(2);
                    string caretakerEmail = reader.GetString(3);
                    reader.Close();

                    double distanceKm = Haversine(data.lat, data.lon, homeLat, homeLng);
                    bool isOutside = distanceKm > 0.001;
                    bool wasOutside = geofenceState.TryGetValue(data.elderId, out bool prev) && prev;

                    if (isOutside && !wasOutside)
                    {
                        geofenceState[data.elderId] = true;
                        await emailService.SendGeofenceAlertAsync(caretakerEmail, elderName, "LEFT");
                        Console.WriteLine($"Geofence LEFT alert sent for Elder {data.elderId}");
                    }
                    else if (!isOutside && wasOutside)
                    {
                        geofenceState[data.elderId] = false;
                        await emailService.SendGeofenceAlertAsync(caretakerEmail, elderName, "RETURNED");
                        Console.WriteLine($"Geofence RETURNED alert sent for Elder {data.elderId}");
                    }
                    else
                    {
                        geofenceState[data.elderId] = isOutside;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Geofence check error: {ex.Message}");
                }
            });
        }
    }
});

app.MapGet("/api/live-locations", () =>
{
    return liveLocations.Select(x => new
    {
        elderId = x.Key,
        lat = x.Value.lat,
        lon = x.Value.lon
    });
}).RequireAuthorization();

app.Map("/ws/video", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    if (!context.Request.Query.TryGetValue("elderId", out var elderIdStr)
        || !int.TryParse(elderIdStr, out int connectedElderId))
    {
        context.Response.StatusCode = 400;
        return;
    }

    using var ws = await context.WebSockets.AcceptWebSocketAsync();

    elderSockets[connectedElderId] = ws;
    Console.WriteLine($"Elder {connectedElderId} connected to video WebSocket");

    var buffer = new byte[1024 * 1024];

    while (true)
    {
        var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
        if (result.MessageType == WebSocketMessageType.Close)
            break;

        var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
        var data = JsonSerializer.Deserialize<JsonElement>(msg);

        string eventType = data.GetProperty("event").GetString();
        int elderId = data.GetProperty("elder_id").GetInt32();

        if (eventType == "STREAM_FRAME" && data.TryGetProperty("image", out var imgProp))
        {
            var frameBytes = Convert.FromBase64String(imgProp.GetString());
            latestFrames[elderId] = frameBytes;
        }
        else if (eventType == "FALL_DETECTED" || eventType == "IDLE_DETECTED")
        {
            Console.WriteLine($"Alert: {eventType} for Elder {elderId}");

            byte[] snapshot = null;
            if (data.TryGetProperty("image", out var snapProp))
                snapshot = Convert.FromBase64String(snapProp.GetString());

            _ = Task.Run(async () =>
            {
                try
                {
                    var db = app.Services.GetRequiredService<DBConnect>();
                    var emailService = app.Services.GetRequiredService<EmailService>();
                    var fcmService = app.Services.GetRequiredService<FcmService>();

                    using var con = db.GetConnection();
                    using var cmd = new SqlCommand(@"
                        SELECT c.Email, c.FirstName, e.elderName, e.FCMToken
                        FROM CaretakerElderMap m
                        JOIN caretakerTable c ON c.CareTakerID = m.CareTakerID
                        JOIN elderTable e ON e.elderId = m.ElderID
                        WHERE m.ElderID = @elderId", con);

                    cmd.Parameters.AddWithValue("@elderId", elderId);
                    con.Open();

                    using var reader = cmd.ExecuteReader();
                    if (!reader.Read()) return;

                    string caretakerEmail = reader["Email"].ToString();
                    string elderName = reader["elderName"].ToString();
                    string fcmToken = reader["FCMToken"]?.ToString();
                    reader.Close();
                    con.Close();

                    await emailService.SendAlertEmailAsync(caretakerEmail, elderName, eventType, snapshot);
                    Console.WriteLine($"Alert email sent to {caretakerEmail}");

                    if (!string.IsNullOrEmpty(fcmToken))
                    {
                        string notifTitle = eventType == "FALL_DETECTED" ? "Fall Detected!" : "Idle Alert!";
                        string notifBody = "Are you okay? Please respond within 60 seconds.";

                        await fcmService.SendNotificationAsync(fcmToken, notifTitle, notifBody, new Dictionary<string, string>
                        {
                            { "elderId", elderId.ToString() },
                            { "eventType", eventType }
                        });

                        var tcs = new TaskCompletionSource<string>();
                        pendingResponses[elderId] = tcs;

                        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(60));
                        var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                        pendingResponses.TryRemove(elderId, out _);

                        if (completedTask == timeoutTask)
                        {
                            await emailService.SendFollowUpEmailAsync(caretakerEmail, elderName, "NO_RESPONSE");
                            Console.WriteLine("No response from elder - follow up email sent");
                        }
                        else
                        {
                            string response = tcs.Task.Result;
                            if (response.ToLower() == "yes")
                            {
                                await emailService.SendFollowUpEmailAsync(caretakerEmail, elderName, "OKAY");
                                Console.WriteLine("Elder is okay - follow up email sent");
                            }
                            else
                            {
                                await emailService.SendFollowUpEmailAsync(caretakerEmail, elderName, "NOT_OKAY");
                                Console.WriteLine("Elder not okay - follow up email sent");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send alert email: {ex.Message}");
                }
            });
        }
    }

    elderSockets.TryRemove(connectedElderId, out _);
    latestFrames.TryRemove(connectedElderId, out _);
    Console.WriteLine($"Elder {connectedElderId} disconnected from video WebSocket");
});

app.MapGet("/stream/{elderId}", async (int elderId, HttpContext context) =>
{
    context.Response.ContentType = "multipart/x-mixed-replace; boundary=frame";

    while (!context.RequestAborted.IsCancellationRequested)
    {
        if (latestFrames.TryGetValue(elderId, out var frame))
        {
            var header = "--frame\r\nContent-Type: image/jpeg\r\n\r\n";
            var headerBytes = Encoding.UTF8.GetBytes(header);
            var footer = Encoding.UTF8.GetBytes("\r\n");

            await context.Response.Body.WriteAsync(headerBytes);
            await context.Response.Body.WriteAsync(frame);
            await context.Response.Body.WriteAsync(footer);
            await context.Response.Body.FlushAsync();
        }
        await Task.Delay(33);
    }
});

app.MapPost("/api/elder/respond", async (HttpContext context) =>
{
    var body = await JsonSerializer.DeserializeAsync<JsonElement>(context.Request.Body);
    int elderId = body.GetProperty("elderId").GetInt32();
    string response = body.GetProperty("response").GetString();

    if (pendingResponses.TryGetValue(elderId, out var tcs))
    {
        tcs.TrySetResult(response);
    }

    return Results.Ok();
});

app.MapPost("/api/elder/start-stream/{elderId}", async (int elderId) =>
{
    if (elderSockets.TryGetValue(elderId, out var ws) && ws.State == WebSocketState.Open)
    {
        var cmd = JsonSerializer.SerializeToUtf8Bytes(new { command = "START_STREAM" });
        await ws.SendAsync(cmd, WebSocketMessageType.Text, true, CancellationToken.None);
        Console.WriteLine($"START_STREAM sent to Elder {elderId}");
        return Results.Ok();
    }
    return Results.NotFound("Elder not connected");
}).RequireAuthorization();

app.MapPost("/api/elder/stop-stream/{elderId}", async (int elderId) =>
{
    if (elderSockets.TryGetValue(elderId, out var ws) && ws.State == WebSocketState.Open)
    {
        var cmd = JsonSerializer.SerializeToUtf8Bytes(new { command = "STOP_STREAM" });
        await ws.SendAsync(cmd, WebSocketMessageType.Text, true, CancellationToken.None);
        Console.WriteLine($"STOP_STREAM sent to Elder {elderId}");
        return Results.Ok();
    }
    return Results.NotFound("Elder not connected");
}).RequireAuthorization();

// NEW: Android app calls this on first login to set elder's home location
// Only sets if HomeLat/HomeLng is NULL — never overwrites existing home
app.MapPost("/api/elder/set-home", async (HttpContext context) =>
{
    try
    {
        var body = await JsonSerializer.DeserializeAsync<JsonElement>(context.Request.Body);
        int elderId = body.GetProperty("elderId").GetInt32();
        double lat = body.GetProperty("lat").GetDouble();
        double lng = body.GetProperty("lng").GetDouble();

        var db = context.RequestServices.GetRequiredService<DBConnect>();
        using var con = db.GetConnection();
        using var cmd = new SqlCommand(@"
            UPDATE elderTable
            SET HomeLat = @lat, HomeLng = @lng
            WHERE elderId = @elderId
            AND (HomeLat IS NULL OR HomeLng IS NULL)", con);

        cmd.Parameters.AddWithValue("@lat", lat);
        cmd.Parameters.AddWithValue("@lng", lng);
        cmd.Parameters.AddWithValue("@elderId", elderId);
        con.Open();
        int rows = cmd.ExecuteNonQuery();

        if (rows > 0)
            Console.WriteLine($"Home location set for Elder {elderId}: {lat}, {lng}");
        else
            Console.WriteLine($"Home location already set for Elder {elderId}, skipping");

        return Results.Ok();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"set-home FULL ERROR: {ex}");
        return Results.Problem(ex.ToString());
    }
});
   //.RequireAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=landing}/{action=landing}/{id?}");

static double Haversine(double lat1, double lon1, double lat2, double lon2)
{
    const double R = 6371;
    double dLat = (lat2 - lat1) * Math.PI / 180;
    double dLon = (lon2 - lon1) * Math.PI / 180;
    double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
               Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
               Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
    return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
}

app.Run();