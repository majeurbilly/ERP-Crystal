namespace Crystal.Core.DTOs.Responses;

public class HrDashboardMetricsDto
{
    public int TotalActiveEmployees { get; set; }
    public int PendingTimesheetsCount { get; set; }
    public int PendingLeaveRequestsCount { get; set; }
    public decimal TotalGrossPayroll { get; set; }
}
