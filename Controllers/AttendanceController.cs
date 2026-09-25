using System.Globalization;
using AttendanceApp.DTOs;
using AttendanceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceApp.Controllers;

[Authorize]
public class AttendanceController : Controller
{
    private const int MaxVisibleMessages = 5;

    private readonly AttendanceService _attendanceService;
    private readonly IExcelAttendanceReader _excelAttendanceReader;

    public AttendanceController(
        AttendanceService attendanceService,
        IExcelAttendanceReader excelAttendanceReader)
    {
        _attendanceService = attendanceService;
        _excelAttendanceReader = excelAttendanceReader;
    }

    // halaman utama "Attendance File": menampilkan data yang sudah tersimpan di database
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

    // membaca file Excel yang di-upload lalu menampilkan isinya di grid untuk diedit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(
        IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] =
                "Please choose an Excel file (*.xlsx) to upload.";

            return RedirectToAction(nameof(Index));
        }

        if (!string.Equals(
                Path.GetExtension(file.FileName),
                ".xlsx",
                StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] =
                $"'{file.FileName}' is not supported. Only .xlsx files are accepted.";

            return RedirectToAction(nameof(Index));
        }

        ExcelAttendanceResult result;

        try
        {
            // dibaca ke memory dulu supaya stream request tidak dipakai saat parsing
            using var stream = new MemoryStream();

            await file.CopyToAsync(stream, HttpContext.RequestAborted);

            stream.Position = 0;

            result = _excelAttendanceReader.Read(stream);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            TempData["Error"] =
                $"'{file.FileName}' could not be read as an Excel workbook: {ex.Message}";

            return RedirectToAction(nameof(Index));
        }

        if (result.Rows.Count == 0)
        {
            TempData["Error"] = result.Errors.Count == 0
                ? $"No attendance data was found in '{file.FileName}'."
                : $"No valid attendance row was found in '{file.FileName}'. {FormatMessages(result.Errors)}";

            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] =
            $"{result.Rows.Count} row(s) loaded from '{file.FileName}'. Edit Attendance IN / OUT / Leave, then click Save.";

        if (result.Errors.Count > 0)
        {
            TempData["Warning"] =
                $"{result.Errors.Count} row(s) were skipped. {FormatMessages(result.Errors)}";
        }

        ViewData["AttendanceDate"] =
            result.Rows[0].AttendanceDate.ToString("yyyy-MM-dd");

        ViewData["SourceFileName"] = file.FileName;

        // grid dirender langsung dari hasil parsing (belum masuk database sampai tombol Save diklik)
        return View(nameof(Index), result.Rows);
    }

    // menyimpan hasil editing grid ke SQL Server (upsert per EmployeeId + AttendanceDate)
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
            TempData["Error"] = BuildModelStateMessage();

            return RedirectToIndex(attendanceDate);
        }

        try
        {
            await _attendanceService.SaveAsync(
                rows,
                HttpContext.RequestAborted);

            var distinctDates = rows
                .Select(row => row.AttendanceDate.Date)
                .Distinct()
                .Count();

            TempData["Success"] = distinctDates > 1
                ? $"Attendance saved successfully. {distinctDates} dates were saved; showing {attendanceDate:yyyy-MM-dd} below."
                : $"Attendance saved successfully. {rows.Count} row(s) saved.";
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

    private string BuildModelStateMessage()
    {
        var messages = ModelState
            .Where(entry => entry.Value != null && entry.Value.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(error =>
                string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? $"{entry.Key}: invalid value."
                    : $"{entry.Key}: {error.ErrorMessage}"))
            .Distinct()
            .ToList();

        return messages.Count == 0
            ? "Some rows are invalid. Please check the submitted values."
            : $"Some rows are invalid -> {FormatMessages(messages)}";
    }

    private static string FormatMessages(
        IReadOnlyList<string> messages)
    {
        var visible = messages
            .Take(MaxVisibleMessages)
            .ToList();

        var text = string.Join(" | ", visible);

        return messages.Count > MaxVisibleMessages
            ? $"{text} (+{messages.Count - MaxVisibleMessages} more)"
            : text;
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
