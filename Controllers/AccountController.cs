using System.Security.Claims;
using AttendanceApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceApp.Controllers;

public class AccountController : Controller
{
    private readonly GoogleAuthOptions _googleAuthOptions;

    public AccountController(GoogleAuthOptions googleAuthOptions)
    {
        _googleAuthOptions = googleAuthOptions;
    }

    // halaman login hanya menyediakan tombol "Sign in with Google",
    // jadi tidak ada form username/password di sisi view.
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
            // aplikasi tetap bisa jalan tanpa Google OAuth; pesan ini tampil di halaman login
            // lewat alert @TempData["Error"] yang sudah ada di Views/Account/Login.cshtml.
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
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (result?.Principal == null)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
            {
                // pesan ditampilkan di halaman login (tidak ada form username/password).
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
