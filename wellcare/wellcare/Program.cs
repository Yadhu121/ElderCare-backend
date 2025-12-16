using Microsoft.AspNetCore.Builder;
using wellcare.Models;
using wellcare.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
//builder.Services.AddControllers();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddSingleton<DBConnect>();
builder.Services.AddScoped<caretakerTable>();
builder.Services.AddScoped<OtpTable>();
builder.Services.AddTransient<EmailService>();
builder.Services.AddScoped<elderTable>();
builder.Services.AddScoped<CaretakerElderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=caretakerLogin}/{action=Login}/{id?}");
//app.MapControllers();

app.Run();
