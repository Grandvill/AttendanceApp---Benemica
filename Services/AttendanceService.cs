public class AttendanceService
{
    private readonly ApplicationDbContext _context;

    public AttendanceService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(IEnumerable<AttendanceRowDto> rows, CancellationToken ct = default)
{
    if (rows is null) return;

    var normalized = rows
        .Where(r => !string.IsNullOrWhiteSpace(r.EmployeeId))
        .GroupBy(r => (r.EmployeeId, Date: r.AttendanceDate.Date))
        .Select(g => g.Last())
        .ToList();

    if (normalized.Count == 0) return;

    var employeeIds = normalized.Select(r => r.EmployeeId).Distinct().ToList();
    var dates = normalized.Select(r => r.AttendanceDate.Date).Distinct().ToList();

    var existing = await _context.Attendances
        .Where(a => employeeIds.Contains(a.EmployeeId) && dates.Contains(a.AttendanceDate))
        .ToListAsync(ct);

    foreach (var row in normalized)

    await _context.SaveChangesAsync(ct);
}

}