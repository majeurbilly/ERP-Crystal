using Crystal.Core.Entities;
using Crystal.Core.Enums;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Repositories;

public class LeaveRequestRepository : RepositoryBase, ILeaveRequestRepository
{
    public LeaveRequestRepository(CrystalDbContext p_context)
        : base(p_context)
    {
    }

    public async Task<int> CountByStatusAsync(LeaveRequestStatus p_status)
    {
        return await m_context.LeaveRequests
            .AsNoTracking()
            .CountAsync(p_request => p_request.Status == p_status);
    }

    public async Task<IEnumerable<LeaveRequest>> GetAllAsync()
    {
        return await m_context.LeaveRequests
            .AsNoTracking()
            .Include(p_request => p_request.EmployeeProfile)
            .OrderByDescending(p_request => p_request.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeProfileIdAsync(int p_employeeProfileId)
    {
        return await m_context.LeaveRequests
            .AsNoTracking()
            .Include(p_request => p_request.EmployeeProfile)
            .Where(p_request => p_request.EmployeeProfileId == p_employeeProfileId)
            .OrderByDescending(p_request => p_request.StartDate)
            .ToListAsync();
    }

    public async Task<LeaveRequest?> GetByIdAsync(int p_id)
    {
        return await m_context.LeaveRequests
            .AsNoTracking()
            .Include(p_request => p_request.EmployeeProfile)
            .FirstOrDefaultAsync(p_request => p_request.Id == p_id);
    }

    public async Task<LeaveRequest?> GetTrackedByIdAsync(int p_id)
    {
        return await m_context.LeaveRequests
            .FirstOrDefaultAsync(p_request => p_request.Id == p_id);
    }

    public async Task<bool> HasOverlappingLeaveAsync(
        int p_employeeProfileId,
        DateOnly p_startDate,
        DateOnly p_endDate,
        int? p_excludeRequestId = null)
    {
        bool hasOverlap = await m_context.LeaveRequests
            .AsNoTracking()
            .Where(p_request => p_request.EmployeeProfileId == p_employeeProfileId)
            .Where(p_request => p_request.Status != LeaveRequestStatus.Rejected)
            .Where(p_request => !p_excludeRequestId.HasValue || p_request.Id != p_excludeRequestId.Value)
            .AnyAsync(p_request =>
                p_startDate <= p_request.EndDate
                && p_request.StartDate <= p_endDate);

        return hasOverlap;
    }

    public async Task AddAsync(LeaveRequest p_leaveRequest)
    {
        await m_context.LeaveRequests.AddAsync(p_leaveRequest);
    }

    public Task UpdateAsync(LeaveRequest p_leaveRequest)
    {
        m_context.LeaveRequests.Update(p_leaveRequest);
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(LeaveRequest p_leaveRequest)
    {
        p_leaveRequest.IsDeleted = true;
        m_context.LeaveRequests.Update(p_leaveRequest);
        return Task.CompletedTask;
    }

    public async Task SoftDeleteExpiredAsync(DateOnly p_today)
    {
        List<LeaveRequest> expiredLeaveRequests = await m_context.LeaveRequests
            .Where(p_request => p_request.EndDate < p_today)
            .ToListAsync();

        if (expiredLeaveRequests.Count == 0)
        {
            return;
        }

        foreach (LeaveRequest leaveRequest in expiredLeaveRequests)
        {
            leaveRequest.IsDeleted = true;
        }

        await m_context.SaveChangesAsync();
    }

}
