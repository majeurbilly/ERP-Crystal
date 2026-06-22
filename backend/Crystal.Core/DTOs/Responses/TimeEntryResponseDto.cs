namespace Crystal.Core.DTOs.Responses;

public class TimeEntryResponseDto
{
    public int Id { get; set; }
    public int EmployeeProfileId { get; set; }
    public string EmployeeFirstName { get; set; } = string.Empty;
    public string EmployeeLastName { get; set; } = string.Empty;
    public int? ScheduledShiftId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
}
