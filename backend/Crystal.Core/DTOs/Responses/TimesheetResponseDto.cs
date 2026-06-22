namespace Crystal.Core.DTOs.Responses;

public class TimesheetResponseDto
{
    public int Id { get; set; }
    public int EmployeeProfileId { get; set; }
    public string EmployeeFirstName { get; set; } = string.Empty;
    public string EmployeeLastName { get; set; } = string.Empty;
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public IList<TimeEntryResponseDto> TimeEntries { get; set; } = new List<TimeEntryResponseDto>();
}
