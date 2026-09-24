namespace AttendanceApp.Models;

public class Attendance
{
    public long Id { get; set; }

    public string EmployeeId { get; set; } = string.Empty;

    public string EmployeeName { get; set; } = string.Empty;

    public DateTime AttendanceDate { get; set; }

    public TimeSpan? AttendanceIn { get; set; }

    public TimeSpan? AttendanceOut { get; set; }

    public bool IsLeave { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}