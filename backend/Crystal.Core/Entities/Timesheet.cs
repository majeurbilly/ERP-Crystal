using Crystal.Core.Enums;

namespace Crystal.Core.Entities;

public class Timesheet
{
    public int Id { get; set; }

    public int EmployeeProfileId { get; set; }
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public TimesheetStatus Status { get; set; } = TimesheetStatus.Draft;
    public bool IsPaid { get; set; } = false;

    public bool IsDeleted { get; set; } = false;

    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}
