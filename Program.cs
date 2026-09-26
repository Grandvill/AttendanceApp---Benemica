using AttendanceApp.Data;
using AttendanceApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Rahasia TIDAK disimpan di appsettings.json karena file itu ikut ter-commit ke git.
// Development : dotnet user-secrets set "Authentication:Google:ClientId" "<nilai>"
// Production  : environment variable Authentication__Google__ClientId / __ClientSecret
// Validasi ditaruh di luar lambda AddGoogle supaya aplikasi langsung gagal saat start,
// bukan baru gagal saat request pertama (lambda AddGoogle dievaluasi secara lazy).
var googleSection = builder.Configuration.GetSection("Authentication:Google");
var googleClientId = googleSection["ClientId"];
var googleClientSecret = googleSection["ClientSecret"];

if (string.IsNullOrWhiteSpace(googleClientId) || string.IsNullOrWhiteSpace(googleClientSecret))
{
    throw new InvalidOperationException(
        "Konfigurasi 'Authentication:Google' belum lengkap. " +
        "Set lewat user secrets (dotnet user-secrets set \"Authentication:Google:ClientId\" \"<nilai>\" " +
        "dan dotnet user-secrets set \"Authentication:Google:ClientSecret\" \"<nilai>\"), " +
        "atau lewat environment variable Authentication__Google__ClientId / Authentication__Google__ClientSecret.");
}

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "AttendanceApp.Auth";
    })
    .AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = "/signin-google";
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<IExcelAttendanceReader, ExcelAttendanceReader>();

var app = builder.Build();

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
