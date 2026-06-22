using Crystal.Core.DTOs.Responses;
using Crystal.Core.Enums;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;

namespace Crystal.Infrastructure.Services;

public class HrMetricsService : IHrMetricsService
{
    private readonly IEmployeeProfileRepository m_employeeProfileRepository;
    private readonly ITimesheetRepository m_timesheetRepository;
    private readonly ILeaveRequestRepository m_leaveRequestRepository;
    private readonly IPayStubRepository m_payStubRepository;

    public HrMetricsService(
        IEmployeeProfileRepository p_employeeProfileRepository,
        ITimesheetRepository p_timesheetRepository,
        ILeaveRequestRepository p_leaveRequestRepository,
        IPayStubRepository p_payStubRepository)
    {
        m_employeeProfileRepository = p_employeeProfileRepository;
        m_timesheetRepository = p_timesheetRepository;
        m_leaveRequestRepository = p_leaveRequestRepository;
        m_payStubRepository = p_payStubRepository;
    }

    public async Task<HrDashboardMetricsDto> GetDashboardMetricsAsync()
    {
        int totalActiveEmployees = await m_employeeProfileRepository.CountActiveAsync();
        int pendingTimesheetsCount = await m_timesheetRepository.CountByStatusAsync(TimesheetStatus.Submitted);
        int pendingLeaveRequestsCount = await m_leaveRequestRepository.CountByStatusAsync(LeaveRequestStatus.Pending);
        decimal totalGrossPayroll = await m_payStubRepository.SumGrossPayAsync();

        return new HrDashboardMetricsDto
        {
            TotalActiveEmployees = totalActiveEmployees,
            PendingTimesheetsCount = pendingTimesheetsCount,
            PendingLeaveRequestsCount = pendingLeaveRequestsCount,
            TotalGrossPayroll = totalGrossPayroll
        };
    }
}
