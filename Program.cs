using AttendanceApp.Data;
using AttendanceApp.Models;
using AttendanceApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;

// Membuat aplikasi dan mendaftarkan MVC
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Development : dotnet user-secrets set "Authentication:Google:ClientId" "<nilai>"
// Production  : environment variable Authentication__Google__ClientId / __ClientSecret

// Membaca konfigurasi Google Authentication
var googleSection = builder.Configuration.GetSection("Authentication:Google");
var googleClientId = googleSection["ClientId"];
var googleClientSecret = googleSection["ClientSecret"];

var isGoogleConfigured = !string.IsNullOrWhiteSpace(googleClientId)
    && !string.IsNullOrWhiteSpace(googleClientSecret);

// Mendaftarkan GoogleAuthOptions
builder.Services.AddSingleton(new GoogleAuthOptions { IsConfigured = isGoogleConfigured });

// Mengatur Authentication dan Cookie
var authentication = builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = isGoogleConfigured
            ? GoogleDefaults.AuthenticationScheme
            : CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "AttendanceApp.Auth";
    });

if (isGoogleConfigured)
{
    authentication.AddGoogle(options =>
    {
        options.ClientId = googleClientId!;
        options.ClientSecret = googleClientSecret!;
        options.CallbackPath = "/signin-google";
    });
}

builder.Services.AddAuthorization();

// Menghubungkan Entity Framework Core ke SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

// Mendaftarkan Service dan Excel Reader
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<IExcelAttendanceReader, ExcelAttendanceReader>();

var app = builder.Build();

// kalau google belum terkonfigurasi
if (!isGoogleConfigured)
{
    app.Logger.LogWarning(
        "Konfigurasi 'Authentication:Google' belum diisi. Aplikasi berjalan tanpa Google OAuth; " +
        "isi lewat 'dotnet user-secrets set \"Authentication:Google:ClientId\" \"<nilai>\"' dan " +
        "'dotnet user-secrets set \"Authentication:Google:ClientSecret\" \"<nilai>\"', lalu restart aplikasi.");
}

// middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();


//routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
