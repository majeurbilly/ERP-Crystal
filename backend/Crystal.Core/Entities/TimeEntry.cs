namespace Crystal.Core.Entities;

public class TimeEntry
{
    public int Id { get; set; }

    public int EmployeeProfileId { get; set; }
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    public int? ScheduledShiftId { get; set; }
    public ScheduledShift? ScheduledShift { get; set; }

    public int? TimesheetId { get; set; }
    public Timesheet? Timesheet { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }

    public bool IsDeleted { get; set; } = false;
}
