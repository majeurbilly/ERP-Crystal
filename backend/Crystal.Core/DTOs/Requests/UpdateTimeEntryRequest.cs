namespace Crystal.Core.DTOs.Requests;

public class UpdateTimeEntryRequest
{
    public int EmployeeProfileId { get; set; }
    public int? ScheduledShiftId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
}
