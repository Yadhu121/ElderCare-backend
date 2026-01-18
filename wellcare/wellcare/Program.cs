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

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseWebSockets(); 

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

var liveLocations = new ConcurrentDictionary<int, (double lat, double lon)>();


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


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=caretakerLogin}/{action=Login}/{id?}");

app.Run();
