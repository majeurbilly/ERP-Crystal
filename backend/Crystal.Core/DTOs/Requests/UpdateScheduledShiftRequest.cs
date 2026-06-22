namespace Crystal.Core.DTOs.Requests;

public class UpdateScheduledShiftRequest
{
    public int? EmployeeProfileId { get; set; }
    public int LocationId { get; set; }
    public int JobPositionId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
