namespace AttendanceApp.DTOs;

public class AttendanceRowDto
{
    public long Id { get; set; }

    public string EmployeeId { get; set; } = string.Empty;

    public string EmployeeName { get; set; } = string.Empty;

    public DateTime AttendanceDate { get; set; }

    public string? AttendanceIn { get; set; }

    public string? AttendanceOut { get; set; }

    public bool IsLeave { get; set; }
}