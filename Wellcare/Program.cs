using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Net.WebSockets;
using System.Text.Json;
using System.Collections.Concurrent;
using wellcare.Models;
using wellcare.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<DBConnect>();
builder.Services.AddScoped<caretakerTable>();
builder.Services.AddScoped<OtpTable>();
builder.Services.AddTransient<EmailService>();
builder.Services.AddScoped<elderTable>();
builder.Services.AddScoped<CaretakerElderService>();
builder.Services.AddScoped<elderProfile>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PrescriptionTable>();

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

    using var ws = await context.WebSockets.AcceptWebSocketAsync();

    var startCmd = JsonSerializer.SerializeToUtf8Bytes(new { command = "START_STREAM" });
    await ws.SendAsync(startCmd, WebSocketMessageType.Text, true, CancellationToken.None);

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
    }
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=landing}/{action=landing}/{id?}");

app.Run();