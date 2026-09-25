using System.Globalization;
using AttendanceApp.DTOs;
using AttendanceApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceApp.Controllers;

public class AttendanceController : Controller
{
    private readonly AttendanceService _attendanceService;

    public AttendanceController(
        AttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? date)
    {
        var attendanceDate = ParseDate(date);

        var rows = await _attendanceService.GetByDateAsync(
            attendanceDate,
            HttpContext.RequestAborted);

        ViewData["AttendanceDate"] =
            attendanceDate.ToString("yyyy-MM-dd");

        return View(rows);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        [FromForm] List<AttendanceRowDto> rows)
    {
        if (rows == null || rows.Count == 0)
        {
            TempData["Error"] =
                "No attendance rows were submitted.";

            return RedirectToAction(nameof(Index));
        }

        var attendanceDate = rows[0].AttendanceDate == default
            ? DateTime.Today
            : rows[0].AttendanceDate.Date;

        if (!ModelState.IsValid)
        {
            TempData["Error"] = BuildValidationMessage();

            return RedirectToIndex(attendanceDate);
        }

        try
        {
            await _attendanceService.SaveAsync(
                rows,
                HttpContext.RequestAborted);

            TempData["Success"] =
                "Attendance saved successfully.";
        }
        catch (FormatException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (DbUpdateException)
        {
            TempData["Error"] =
                "Attendance could not be saved. Please check the submitted values.";
        }

        return RedirectToIndex(attendanceDate);
    }

    private IActionResult RedirectToIndex(
        DateTime attendanceDate)
    {
        return RedirectToAction(
            nameof(Index),
            new { date = attendanceDate.ToString("yyyy-MM-dd") });
    }

    private string BuildValidationMessage()
    {
        var messages = ModelState
            .Where(entry => entry.Value != null && entry.Value.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(error =>
                string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? $"{entry.Key}: invalid value."
                    : $"{entry.Key}: {error.ErrorMessage}"))
            .Distinct()
            .Take(5)
            .ToList();

        return messages.Count == 0
            ? "Some rows are invalid. Please check the submitted values."
            : $"Some rows are invalid -> {string.Join(" | ", messages)}";
    }

    // membaca tanggal dari query string (format yyyy-MM-dd dari input type="date"),
    // kembali ke tanggal hari ini bila kosong atau tidak valid
    private static DateTime ParseDate(
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)
            && DateTime.TryParseExact(
                value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
        {
            return parsed.Date;
        }

        return DateTime.Today;
    }
}
