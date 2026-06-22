using Crystal.Core.Enums;

namespace Crystal.Core.Entities;

public class LeaveRequest
{
    public int Id { get; set; }

    public int EmployeeProfileId { get; set; }
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    public LeaveType LeaveType { get; set; }
    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Reason { get; set; }

    public bool IsDeleted { get; set; } = false;
}
