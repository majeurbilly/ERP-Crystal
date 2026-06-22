using Crystal.Core.Entities;
using Crystal.Core.Enums;

namespace Crystal.Core.Interfaces.Repositories;

public interface ILeaveRequestRepository
{
    Task<int> CountByStatusAsync(LeaveRequestStatus p_status);
    Task<IEnumerable<LeaveRequest>> GetAllAsync();
    Task<IEnumerable<LeaveRequest>> GetByEmployeeProfileIdAsync(int p_employeeProfileId);
    Task<LeaveRequest?> GetByIdAsync(int p_id);
    Task<LeaveRequest?> GetTrackedByIdAsync(int p_id);
    Task<bool> HasOverlappingLeaveAsync(int p_employeeProfileId, DateOnly p_startDate, DateOnly p_endDate, int? p_excludeRequestId = null);
    Task AddAsync(LeaveRequest p_leaveRequest);
    Task UpdateAsync(LeaveRequest p_leaveRequest);
    Task SoftDeleteAsync(LeaveRequest p_leaveRequest);
    Task SoftDeleteExpiredAsync(DateOnly p_today);
    Task SaveChangesAsync();
}
