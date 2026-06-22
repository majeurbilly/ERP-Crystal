namespace Crystal.Core.DTOs.Responses;

public class ScheduledShiftResponseDto
{
    public int Id { get; set; }
    public int? EmployeeProfileId { get; set; }
    public string? EmployeeFirstName { get; set; }
    public string? EmployeeLastName { get; set; }
    public int JobPositionId { get; set; }
    public string JobPositionName { get; set; } = string.Empty;
    public string JobPositionColor { get; set; } = string.Empty;
    public int? LocationId { get; set; }
    public string? LocationTitle { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
