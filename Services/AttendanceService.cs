using System.Globalization;
using AttendanceApp.Data;
using AttendanceApp.DTOs;
using AttendanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AttendanceApp.Services;

public class AttendanceService
{
    private readonly ApplicationDbContext _context;

    public AttendanceService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    // mengambil daftar attendance pada satu tanggal untuk ditampilkan di halaman Index
    public async Task<List<AttendanceRowDto>> GetByDateAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var startDate = date.Date;
        var endDate = startDate.AddDays(1);

        var attendances = await _context.Attendances
            .AsNoTracking()
            .Where(x => x.AttendanceDate >= startDate
                && x.AttendanceDate < endDate)
            .OrderBy(x => x.EmployeeId)
            .ToListAsync(cancellationToken);

        return attendances
            .Select(x => new AttendanceRowDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                EmployeeName = x.EmployeeName,
                AttendanceDate = x.AttendanceDate.Date,
                AttendanceIn = ToTimeText(x.AttendanceIn),
                AttendanceOut = ToTimeText(x.AttendanceOut),
                IsLeave = x.IsLeave
            })
            .ToList();
    }

    public async Task SaveAsync(
        List<AttendanceRowDto> rows,
        CancellationToken cancellationToken = default)
    {
        if (rows == null || rows.Count == 0)
        {
            return;
        }

        // baris dengan EmployeeId + tanggal yang sama dalam satu request digabung
        // supaya tidak menghasilkan dua record untuk kombinasi yang sama
        var normalizedRows = rows
            .Where(row => !string.IsNullOrWhiteSpace(row.EmployeeId))
            .GroupBy(row => (row.EmployeeId, Date: row.AttendanceDate.Date))
            .Select(group => group.Last())
            .ToList();

        if (normalizedRows.Count == 0)
        {
            return;
        }

        var startDate = normalizedRows.Min(row => row.AttendanceDate.Date);
        var endDate = normalizedRows.Max(row => row.AttendanceDate.Date).AddDays(1);

        var employeeIds = normalizedRows
            .Select(row => row.EmployeeId)
            .Distinct()
            .ToList();

        // satu query untuk semua baris, bukan satu query per baris (menghindari N+1)
        var existingAttendances = await _context.Attendances
            .Where(x => employeeIds.Contains(x.EmployeeId)
                && x.AttendanceDate >= startDate
                && x.AttendanceDate < endDate)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        foreach (var row in normalizedRows)
        {
            var attendanceDate = row.AttendanceDate.Date;

            var attendance = existingAttendances.FirstOrDefault(
                x => x.EmployeeId == row.EmployeeId
                    && x.AttendanceDate.Date == attendanceDate);

            if (attendance == null)
            {
                attendance = new Attendance
                {
                    EmployeeId = row.EmployeeId.Trim(),
                    EmployeeName = (row.EmployeeName ?? string.Empty).Trim(),
                    AttendanceDate = attendanceDate,
                    AttendanceIn = ParseTime(row.AttendanceIn, nameof(row.AttendanceIn)),
                    AttendanceOut = ParseTime(row.AttendanceOut, nameof(row.AttendanceOut)),
                    IsLeave = row.IsLeave,
                    CreatedAt = now
                };

                _context.Attendances.Add(attendance);

                // supaya baris berikutnya dengan kunci yang sama ikut di-update, bukan di-insert lagi
                existingAttendances.Add(attendance);
            }
            else
            {
                attendance.AttendanceIn =
                    ParseTime(row.AttendanceIn, nameof(row.AttendanceIn));

                attendance.AttendanceOut =
                    ParseTime(row.AttendanceOut, nameof(row.AttendanceOut));

                attendance.IsLeave =
                    row.IsLeave;

                attendance.UpdatedAt =
                    now;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    // mengubah nilai waktu dari form ("08:30" atau "08:30:00") menjadi TimeSpan.
    // nilai kosong dianggap null, nilai tidak valid dilempar sebagai FormatException
    // supaya data tidak tersimpan diam-diam salah
    private static TimeSpan? ParseTime(
        string? value,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        if (TimeSpan.TryParseExact(
                trimmed,
                new[] { @"hh\:mm", @"hh\:mm\:ss" },
                CultureInfo.InvariantCulture,
                out var exact)
            || TimeSpan.TryParse(
                trimmed,
                CultureInfo.InvariantCulture,
                out exact))
        {
            return exact;
        }

        throw new FormatException(
            $"Nilai {fieldName} '{value}' bukan format waktu yang valid (contoh: 08:30).");
    }

    private static string? ToTimeText(TimeSpan? value)
    {
        return value?.ToString(@"hh\:mm");
    }
}
