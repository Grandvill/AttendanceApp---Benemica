using System.Globalization;
using AttendanceApp.DTOs;
using ClosedXML.Excel;

namespace AttendanceApp.Services;

// Membaca file Excel absensi (sheet pertama) menjadi daftar AttendanceRowDto.

// Kolom dicari berdasarkan nama header

// dan baris yang datanya tidak valid hanya dilaporkan, tidak membatalkan seluruh file.
public class ExcelAttendanceReader : IExcelAttendanceReader
{
    private const string ColumnId = "id";
    private const string ColumnName = "name";
    private const string ColumnDate = "date";
    private const string ColumnIn = "in";
    private const string ColumnOut = "out";
    private const string ColumnLeave = "leave";

    private static readonly string[] DateFormats =
    {
        "yyyy-MM-dd", "yyyy/MM/dd", "dd/MM/yyyy", "dd-MM-yyyy"
    };

    private static readonly string[] TimeFormats =
    {
        @"hh\:mm", @"h\:mm", @"hh\:mm\:ss"
    };

    private static readonly Dictionary<string, string[]> HeaderAliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [ColumnId] = new[] { "id", "employeeid", "employee", "employeecode", "nik" },
            [ColumnName] = new[] { "nama", "name", "employeename", "namakaryawan" },
            [ColumnDate] = new[] { "date", "tanggal", "attendancedate" },
            [ColumnIn] = new[] { "attendancein", "in", "timein", "checkin", "clockin", "jammasuk" },
            [ColumnOut] = new[] { "attendanceout", "out", "timeout", "checkout", "clockout", "jamkeluar" },
            [ColumnLeave] = new[] { "leave", "isleave", "cuti", "izin" }
        };

    public ExcelAttendanceResult Read(Stream stream)
    {
        var result = new ExcelAttendanceResult();

        using var workbook = new XLWorkbook(stream);

        var worksheet = workbook.Worksheets.FirstOrDefault();

        if (worksheet == null)
        {
            result.Errors.Add("The workbook does not contain any worksheet.");

            return result;
        }

        var usedRange = worksheet.RangeUsed();

        if (usedRange == null)
        {
            result.Errors.Add("The worksheet is empty.");

            return result;
        }

        // baris pertama yang terpakai dianggap sebagai header
        var headerRowNumber = usedRange.FirstRow().RowNumber();
        var lastRowNumber = usedRange.LastRow().RowNumber();

        if (lastRowNumber <= headerRowNumber)
        {
            result.Errors.Add("The worksheet only contains a header row, no attendance data was found.");

            return result;
        }

        var columns = ResolveColumns(worksheet.Row(headerRowNumber));

        var missingColumns = new[] { ColumnId, ColumnName, ColumnDate }
            .Where(column => !columns.ContainsKey(column))
            .Select(ToHeaderLabel)
            .ToList();

        if (missingColumns.Count > 0)
        {
            result.Errors.Add(
                $"Missing required column(s): {string.Join(", ", missingColumns)}.");

            return result;
        }

        for (var rowNumber = headerRowNumber + 1; rowNumber <= lastRowNumber; rowNumber++)
        {
            var row = worksheet.Row(rowNumber);

            if (row.IsEmpty())
            {
                continue;
            }

            ReadRow(worksheet, rowNumber, columns, result);
        }

        return result;
    }

    private static void ReadRow(
        IXLWorksheet worksheet,
        int rowNumber,
        IReadOnlyDictionary<string, int> columns,
        ExcelAttendanceResult result)
    {
        var rowErrors = new List<string>();

        var employeeId = ReadText(GetCell(worksheet, rowNumber, columns, ColumnId));

        if (string.IsNullOrWhiteSpace(employeeId))
        {
            rowErrors.Add("Employee ID is empty.");
        }

        var employeeName = ReadText(GetCell(worksheet, rowNumber, columns, ColumnName));

        if (string.IsNullOrWhiteSpace(employeeName))
        {
            rowErrors.Add("Employee name is empty.");
        }

        var (attendanceDate, dateError) = ReadDate(GetCell(worksheet, rowNumber, columns, ColumnDate));

        if (dateError != null)
        {
            rowErrors.Add(dateError);
        }

        var (attendanceIn, inError) = ReadTimeText(GetCell(worksheet, rowNumber, columns, ColumnIn));

        if (inError != null)
        {
            rowErrors.Add(inError);
        }

        var (attendanceOut, outError) = ReadTimeText(GetCell(worksheet, rowNumber, columns, ColumnOut));

        if (outError != null)
        {
            rowErrors.Add(outError);
        }

        var (isLeave, leaveError) = ReadLeave(GetCell(worksheet, rowNumber, columns, ColumnLeave));

        if (leaveError != null)
        {
            rowErrors.Add(leaveError);
        }

        if (rowErrors.Count > 0)
        {
            result.Errors.Add($"Row {rowNumber}: {string.Join(" ", rowErrors)}");

            return;
        }

        result.Rows.Add(new AttendanceRowDto
        {
            EmployeeId = employeeId!.Trim(),
            EmployeeName = employeeName!.Trim(),
            AttendanceDate = attendanceDate!.Value,
            AttendanceIn = attendanceIn,
            AttendanceOut = attendanceOut,
            IsLeave = isLeave
        });
    }

    private static Dictionary<string, int> ResolveColumns(IXLRow headerRow)
    {
        var columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var cell in headerRow.CellsUsed())
        {
            var header = NormalizeHeader(cell.GetString());

            if (header.Length == 0)
            {
                continue;
            }

            var columnKey = ResolveColumnKey(header);

            if (columnKey != null && !columns.ContainsKey(columnKey))
            {
                columns[columnKey] = cell.Address.ColumnNumber;
            }
        }

        return columns;
    }

    private static string? ResolveColumnKey(string header)
    {
        foreach (var pair in HeaderAliases)
        {
            if (pair.Value.Contains(header, StringComparer.OrdinalIgnoreCase))
            {
                return pair.Key;
            }
        }

        return null;
    }

    private static string NormalizeHeader(string header)
    {
        return new string(header
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray());
    }

    private static IXLCell? GetCell(
        IXLWorksheet worksheet,
        int rowNumber,
        IReadOnlyDictionary<string, int> columns,
        string columnKey)
    {
        return columns.TryGetValue(columnKey, out var columnNumber)
            ? worksheet.Cell(rowNumber, columnNumber)
            : null;
    }

    private static string? ReadText(IXLCell? cell)
    {
        if (cell == null || cell.IsEmpty())
        {
            return null;
        }

        // ID bisa tersimpan sebagai angka di Excel (10001) sehingga diformat tanpa desimal
        if (cell.DataType == XLDataType.Number)
        {
            return cell.Value.GetNumber().ToString("0", CultureInfo.InvariantCulture);
        }

        return cell.GetString().Trim();
    }

    private static (DateTime? Date, string? Error) ReadDate(IXLCell? cell)
    {
        if (cell == null || cell.IsEmpty())
        {
            return (null, "Date is empty.");
        }

        if (cell.DataType == XLDataType.DateTime)
        {
            return (cell.Value.GetDateTime().Date, null);
        }

        var text = cell.GetString().Trim();

        if (DateTime.TryParseExact(
                text,
                DateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var exact))
        {
            return (exact.Date, null);
        }

        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            || DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsed))
        {
            return (parsed.Date, null);
        }

        return (null, $"Date '{text}' is not a valid date (expected yyyy-MM-dd).");
    }

    // mengembalikan jam dalam format "HH:mm"
    private static (string? Text, string? Error) ReadTimeText(IXLCell? cell)
    {
        if (cell == null || cell.IsEmpty())
        {
            return (null, null);
        }

        // Excel menyimpan jam sebagai TimeSpan, DateTime, atau pecahan hari (0.3368 = 08:05)
        var time = cell.DataType switch
        {
            XLDataType.TimeSpan => cell.Value.GetTimeSpan(),
            XLDataType.DateTime => cell.Value.GetDateTime().TimeOfDay,
            XLDataType.Number => TimeSpan.FromDays(cell.Value.GetNumber()),
            _ => (TimeSpan?)null
        };

        if (time.HasValue)
        {
            return (FormatTime(time.Value), null);
        }

        var text = cell.GetString().Trim();

        if (TimeSpan.TryParseExact(text, TimeFormats, CultureInfo.InvariantCulture, out var exact)
            || TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out exact))
        {
            return (FormatTime(exact), null);
        }

        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
        {
            return (FormatTime(dateTime.TimeOfDay), null);
        }

        return (null, $"Time '{text}' is not a valid time (expected HH:mm).");
    }

    private static (bool IsLeave, string? Error) ReadLeave(IXLCell? cell)
    {
        if (cell == null || cell.IsEmpty())
        {
            return (false, null);
        }

        if (cell.DataType == XLDataType.Boolean)
        {
            return (cell.Value.GetBoolean(), null);
        }

        var text = cell.GetString().Trim().ToLowerInvariant();

        return text switch
        {
            "yes" or "y" or "true" or "1" or "leave" or "cuti" or "izin" => (true, null),
            "no" or "n" or "false" or "0" or "-" => (false, null),
            _ => (false, $"Leave '{text}' is not valid (expected Yes or No).")
        };
    }

    // dibulatkan ke menit terdekat supaya pecahan angka dari Excel (0.3368055...) tidak jadi 08:04
    private static string FormatTime(TimeSpan value)
    {
        if (value < TimeSpan.Zero)
        {
            value = TimeSpan.Zero;
        }

        var totalDays = Math.Floor(value.TotalDays);

        if (totalDays >= 1)
        {
            // kolom SQL bertipe "time" hanya menerima 00:00:00 sampai 23:59:59
            value -= TimeSpan.FromDays(totalDays);
        }

        var rounded = TimeSpan.FromMinutes(Math.Round(value.TotalMinutes, MidpointRounding.AwayFromZero));

        if (rounded.TotalDays >= 1)
        {
            rounded = new TimeSpan(23, 59, 0);
        }

        return rounded.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
    }

    private static string ToHeaderLabel(string columnKey)
    {
        return columnKey switch
        {
            ColumnId => "ID",
            ColumnName => "Nama",
            ColumnDate => "Date",
            ColumnIn => "Attendance IN",
            ColumnOut => "Attendance OUT",
            ColumnLeave => "Leave",
            _ => columnKey
        };
    }


}
