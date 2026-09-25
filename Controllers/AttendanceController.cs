[HttpPost]
public async Task<IActionResult> Save(
    List<AttendanceRowDto> rows)
{
    await _attendanceService.SaveAsync(rows);

    TempData["Success"] =
        "Attendance saved successfully.";

    return RedirectToAction(nameof(Index));
}