using AttendanceApp.Data;
using AttendanceApp.Models;
using AttendanceApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Development : dotnet user-secrets set "Authentication:Google:ClientId" "<nilai>"
// Production  : environment variable Authentication__Google__ClientId / __ClientSecret

// AccountController.GoogleLogin akan menampilkan pesan bahwa konfigurasinya belum ada.
var googleSection = builder.Configuration.GetSection("Authentication:Google");
var googleClientId = googleSection["ClientId"];
var googleClientSecret = googleSection["ClientSecret"];

var isGoogleConfigured = !string.IsNullOrWhiteSpace(googleClientId)
    && !string.IsNullOrWhiteSpace(googleClientSecret);

// status dihitung sekali di sini supaya controller tidak mengulang aturan yang sama.
builder.Services.AddSingleton(new GoogleAuthOptions { IsConfigured = isGoogleConfigured });

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

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<IExcelAttendanceReader, ExcelAttendanceReader>();

var app = builder.Build();

if (!isGoogleConfigured)
{
    app.Logger.LogWarning(
        "Konfigurasi 'Authentication:Google' belum diisi. Aplikasi berjalan tanpa Google OAuth; " +
        "isi lewat 'dotnet user-secrets set \"Authentication:Google:ClientId\" \"<nilai>\"' dan " +
        "'dotnet user-secrets set \"Authentication:Google:ClientSecret\" \"<nilai>\"', lalu restart aplikasi.");
}

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
