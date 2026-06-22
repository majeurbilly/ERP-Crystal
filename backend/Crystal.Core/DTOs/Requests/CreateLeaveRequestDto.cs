using Crystal.Core.Enums;

namespace Crystal.Core.DTOs.Requests;

public class CreateLeaveRequestDto
{
    public int EmployeeProfileId { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Reason { get; set; }
}
