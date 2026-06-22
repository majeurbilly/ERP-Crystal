using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Repositories;

public class ScheduledShiftRepository : RepositoryBase, IScheduledShiftRepository
{
    public ScheduledShiftRepository(CrystalDbContext p_context)
        : base(p_context)
    {
    }

    public async Task<IEnumerable<ScheduledShift>> GetAllAsync()
    {
        return await m_context.ScheduledShifts
            .AsNoTracking()
            .Include(p_shift => p_shift.EmployeeProfile)
            .Include(p_shift => p_shift.Location)
            .Include(p_shift => p_shift.JobPosition)
            .OrderBy(p_shift => p_shift.Date)
            .ThenBy(p_shift => p_shift.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ScheduledShift>> GetByEmployeeProfileIdAsync(int p_employeeProfileId)
    {
        return await m_context.ScheduledShifts
            .AsNoTracking()
            .Include(p_shift => p_shift.EmployeeProfile)
            .Include(p_shift => p_shift.Location)
            .Include(p_shift => p_shift.JobPosition)
            .Where(p_shift => p_shift.EmployeeProfileId == p_employeeProfileId)
            .OrderBy(p_shift => p_shift.Date)
            .ThenBy(p_shift => p_shift.StartTime)
            .ToListAsync();
    }

    public async Task<ScheduledShift?> GetByEmployeeProfileIdAndDateAsync(int p_employeeProfileId, DateOnly p_date)
    {
        return await m_context.ScheduledShifts
            .AsNoTracking()
            .Where(p_shift => p_shift.EmployeeProfileId == p_employeeProfileId && p_shift.Date == p_date)
            .OrderBy(p_shift => p_shift.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task<ScheduledShift?> GetByIdAsync(int p_id)
    {
        return await m_context.ScheduledShifts
            .AsNoTracking()
            .Include(p_shift => p_shift.EmployeeProfile)
            .Include(p_shift => p_shift.Location)
            .Include(p_shift => p_shift.JobPosition)
            .FirstOrDefaultAsync(p_shift => p_shift.Id == p_id);
    }

    public async Task<ScheduledShift?> GetTrackedByIdAsync(int p_id)
    {
        return await m_context.ScheduledShifts
            .FirstOrDefaultAsync(p_shift => p_shift.Id == p_id);
    }

    public async Task AddAsync(ScheduledShift p_scheduledShift)
    {
        await m_context.ScheduledShifts.AddAsync(p_scheduledShift);
    }

    public Task UpdateAsync(ScheduledShift p_scheduledShift)
    {
        m_context.ScheduledShifts.Update(p_scheduledShift);
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(ScheduledShift p_scheduledShift)
    {
        p_scheduledShift.IsDeleted = true;
        m_context.ScheduledShifts.Update(p_scheduledShift);
        return Task.CompletedTask;
    }

}
