namespace Crystal.Core.DTOs.Requests;

public class CreateScheduledShiftRequest
{
    public int? EmployeeProfileId { get; set; }
    public int LocationId { get; set; }
    public int JobPositionId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
