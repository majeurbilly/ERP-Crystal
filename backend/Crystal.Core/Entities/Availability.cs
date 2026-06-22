namespace Crystal.Core.Entities;

public class Availability
{
    public int Id { get; set; }

    public int EmployeeProfileId { get; set; }
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    public DateOnly AvailabilityDate { get; set; }

    public bool IsRecurring { get; set; }
    public DayOfWeek WeekDay { get; set; }

    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
