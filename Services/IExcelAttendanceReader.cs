using AttendanceApp.DTOs;

namespace AttendanceApp.Services;

// interface untuk menampilkan hasil pembacaan file Excel dari AttendanceController
// hasil pembacaan file Excel: daftar AttendanceRowDto
public class ExcelAttendanceResult
{
    public List<AttendanceRowDto> Rows { get; } = new();

    public List<string> Errors { get; } = new();
}

public interface IExcelAttendanceReader
{
    ExcelAttendanceResult Read(Stream stream);
}
