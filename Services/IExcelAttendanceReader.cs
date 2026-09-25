using AttendanceApp.DTOs;

namespace AttendanceApp.Services;

// interface untuk menampilkan hasil pembacaan file Excel dari service ExcelAttendanceReader
// hasil pembacaan file Excel: baris yang valid + pesan kesalahan per baris yang di-skip
public class ExcelAttendanceResult
{
    public List<AttendanceRowDto> Rows { get; } = new();

    public List<string> Errors { get; } = new();
}

public interface IExcelAttendanceReader
{
    ExcelAttendanceResult Read(Stream stream);
}
