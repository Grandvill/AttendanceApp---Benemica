using System.Security.Claims;
using AttendanceApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceApp.Controllers;

public class AccountController : Controller
{
    // mengecek apakah GoogleAuthOptions diisi di Program.cs
    private readonly GoogleAuthOptions _googleAuthOptions;

    public AccountController(GoogleAuthOptions googleAuthOptions)
    {
        _googleAuthOptions = googleAuthOptions;
    }

    // menampilkan halaman login
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GoogleLogin()
    {
        if (!_googleAuthOptions.IsConfigured)
        {
            TempData["Error"] =
                "Login dengan Google belum dikonfigurasi. Isi Authentication:Google:ClientId dan " +
                "Authentication:Google:ClientSecret (user secrets atau environment variable) " +
                "lalu restart aplikasi.";

            return RedirectToAction(nameof(Login));
        }

        var redirectUrl = Url.Action(nameof(GoogleResponse), "Account");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public async Task<IActionResult> GoogleResponse()
    {
        // Mengecek apakah sudah memiliki cookie login
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Jika belum ada cookie, periksa hasil autentikasi Google
        if (result?.Principal == null)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
            {
                // pesan ditampilkan di halaman login
                TempData["Error"] = "Login dengan Google gagal. Silakan coba lagi.";
                return RedirectToAction(nameof(Login));
            }

            var principal = authenticateResult.Principal;
            var claims = principal.Claims.ToList();
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });
        }

        return RedirectToAction("Index", "Home");
    }

    // menghapus autentikasi cookie aplikasi
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
