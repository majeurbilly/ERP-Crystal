using Crystal.Core.Enums;

namespace Crystal.Core.DTOs.Requests;

public class UpdateTimesheetStatusRequest
{
    public TimesheetStatus Status { get; set; }
}
