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

    // get data absensi berdasarkan tgl
    public async Task<List<AttendanceRowDto>> GetByDateAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        // rentang tgl
        var startDate = date.Date;
        var endDate = startDate.AddDays(1);

        // mengakses kumpulan data absensi di tabel database
        var attendances = await _context.Attendances
            .AsNoTracking()
            .Where(x => x.AttendanceDate >= startDate
                && x.AttendanceDate < endDate)
            .OrderBy(x => x.EmployeeId)
            .ToListAsync(cancellationToken);
        
        // Mengubah Entity menjadi DTO
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

    //membersihkan dan menggabungkan data duplikat, mengambil record yang sudah ada, lalu melakukan insert atau update sebelum menyimpan perubahan
    public async Task SaveAsync(
        List<AttendanceRowDto> rows,
        CancellationToken cancellationToken = default)
    {
        // Memeriksa data kosong
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

        // Menentukan rentang tanggal dan daftar karyawan
        var startDate = normalizedRows.Min(row => row.AttendanceDate.Date);
        var endDate = normalizedRows.Max(row => row.AttendanceDate.Date).AddDays(1);

        var employeeIds = normalizedRows
            .Select(row => row.EmployeeId)
            .Distinct()
            .ToList();

        // mencari absensi yg sudah disimpan di database untuk karyawan dan tanggal yang sama
        var existingAttendances = await _context.Attendances
            .Where(x => employeeIds.Contains(x.EmployeeId)
                && x.AttendanceDate >= startDate
                && x.AttendanceDate < endDate)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        // Insert atau update
        foreach (var row in normalizedRows)
        {
            var attendanceDate = row.AttendanceDate.Date;

            var attendance = existingAttendances.FirstOrDefault(
                x => x.EmployeeId == row.EmployeeId
                    && x.AttendanceDate.Date == attendanceDate);

            // kalau belum ada: Insert
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
            // kalau sudah ada: Update
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

    // mengubah input waktu dari form yang berbentuk string menjadi TimeSpan
    // contoh : "08:30" menjadi TimeSpan 08:30:00
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

    // mengubah waktu dari database menjadi string untuk ditampilkan di UI
    // contoh: TimeSpan 08:30:00 menjadi "08:30"
    private static string? ToTimeText(TimeSpan? value)
    {
        return value?.ToString(@"hh\:mm");
    }
}
