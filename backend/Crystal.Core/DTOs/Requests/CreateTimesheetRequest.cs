namespace Crystal.Core.DTOs.Requests;

public class CreateTimesheetRequest
{
    public int EmployeeProfileId { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public IList<int> TimeEntryIds { get; set; } = new List<int>();
}
