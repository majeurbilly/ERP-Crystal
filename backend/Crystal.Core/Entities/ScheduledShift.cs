namespace Crystal.Core.Entities;

public class ScheduledShift
{
    public int Id { get; set; }

    public int? EmployeeProfileId { get; set; }
    public EmployeeProfile? EmployeeProfile { get; set; }

    public int? LocationId { get; set; }
    public Location? Location { get; set; }

    public int JobPositionId { get; set; }
    public JobPosition JobPosition { get; set; } = null!;

    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public bool IsDeleted { get; set; } = false;

    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}
