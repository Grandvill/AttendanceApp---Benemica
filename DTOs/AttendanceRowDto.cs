using System.ComponentModel.DataAnnotations;

namespace AttendanceApp.DTOs;

public class AttendanceRowDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Employee ID is required.")]
    [StringLength(50, ErrorMessage = "Employee ID maximum length is 50 characters.")]
    public string EmployeeId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Employee name is required.")]
    [StringLength(150, ErrorMessage = "Employee name maximum length is 150 characters.")]
    public string EmployeeName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime AttendanceDate { get; set; }

    [DataType(DataType.Time)]
    [RegularExpression(
        @"^$|^([01]?\d|2[0-3]):[0-5]\d(:[0-5]\d)?$",
        ErrorMessage = "Use HH:mm format (e.g. 08:30).")]
    public string? AttendanceIn { get; set; }

    [DataType(DataType.Time)]
    [RegularExpression(
        @"^$|^([01]?\d|2[0-3]):[0-5]\d(:[0-5]\d)?$",
        ErrorMessage = "Use HH:mm format (e.g. 17:00).")]
    public string? AttendanceOut { get; set; }

    public bool IsLeave { get; set; }
}
