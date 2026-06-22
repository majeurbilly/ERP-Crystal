namespace Crystal.Core.Entities;

public class EmployeeProfile
{
    public int Id { get; set; }

    public string? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public decimal Salary { get; set; }
    public string Status { get; set; } = string.Empty;

    public int PositionId { get; set; }
    public JobPosition JobPosition { get; set; } = null!;

    public DateOnly HiringDate { get; set; }

    public int? LocationId { get; set; }
    public Location? Location { get; set; }

    public bool IsDeleted { get; set; } = false;

    public ICollection<ScheduledShift> ScheduledShifts { get; set; } = new List<ScheduledShift>();
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    public ICollection<Timesheet> Timesheets { get; set; } = new List<Timesheet>();
    public ICollection<EmploymentContract> EmploymentContracts { get; set; } = new List<EmploymentContract>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<PayStub> PayStubs { get; set; } = new List<PayStub>();
    public ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
}
