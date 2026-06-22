using Crystal.Core.Entities;
using Crystal.Core.Enums;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Repositories;

public class TimesheetRepository : RepositoryBase, ITimesheetRepository
{
    public TimesheetRepository(CrystalDbContext p_context)
        : base(p_context)
    {
    }

    public async Task<int> CountByStatusAsync(TimesheetStatus p_status)
    {
        return await m_context.Timesheets
            .AsNoTracking()
            .CountAsync(p_timesheet => p_timesheet.Status == p_status);
    }

    public async Task<IEnumerable<Timesheet>> GetAllAsync()
    {
        return await m_context.Timesheets
            .AsNoTracking()
            .Include(p_timesheet => p_timesheet.EmployeeProfile)
            .Include(p_timesheet => p_timesheet.TimeEntries)
                .ThenInclude(p_entry => p_entry.EmployeeProfile)
            .OrderByDescending(p_timesheet => p_timesheet.PeriodStart)
            .ToListAsync();
    }

    public async Task<IEnumerable<Timesheet>> GetByEmployeeProfileIdAsync(int p_employeeProfileId)
    {
        return await m_context.Timesheets
            .AsNoTracking()
            .Include(p_timesheet => p_timesheet.EmployeeProfile)
            .Include(p_timesheet => p_timesheet.TimeEntries)
                .ThenInclude(p_entry => p_entry.EmployeeProfile)
            .Where(p_timesheet => p_timesheet.EmployeeProfileId == p_employeeProfileId)
            .OrderByDescending(p_timesheet => p_timesheet.PeriodStart)
            .ToListAsync();
    }

    public async Task<IList<Timesheet>> GetByPeriodAsync(DateOnly p_periodStart, DateOnly p_periodEnd)
    {
        return await m_context.Timesheets
            .AsNoTracking()
            .Include(p_timesheet => p_timesheet.EmployeeProfile)
            .Include(p_timesheet => p_timesheet.TimeEntries)
                .ThenInclude(p_entry => p_entry.EmployeeProfile)
            .Where(p_timesheet =>
                p_timesheet.PeriodStart == p_periodStart
                && p_timesheet.PeriodEnd == p_periodEnd)
            .ToListAsync();
    }

    public async Task<Timesheet?> GetByIdAsync(int p_id)
    {
        return await m_context.Timesheets
            .AsNoTracking()
            .Include(p_timesheet => p_timesheet.EmployeeProfile)
            .Include(p_timesheet => p_timesheet.TimeEntries)
                .ThenInclude(p_entry => p_entry.EmployeeProfile)
            .FirstOrDefaultAsync(p_timesheet => p_timesheet.Id == p_id);
    }

    public async Task<Timesheet?> GetApprovedByEmployeeAndPeriodAsync(
        int p_employeeProfileId,
        DateOnly p_periodStart,
        DateOnly p_periodEnd)
    {
        return await m_context.Timesheets
            .AsNoTracking()
            .Include(p_timesheet => p_timesheet.TimeEntries)
            .FirstOrDefaultAsync(p_timesheet =>
                p_timesheet.EmployeeProfileId == p_employeeProfileId
                && p_timesheet.PeriodStart == p_periodStart
                && p_timesheet.PeriodEnd == p_periodEnd
                && p_timesheet.Status == TimesheetStatus.Approved);
    }

    public async Task<Timesheet?> GetTrackedByIdAsync(int p_id)
    {
        return await m_context.Timesheets
            .Include(p_timesheet => p_timesheet.EmployeeProfile)
            .FirstOrDefaultAsync(p_timesheet => p_timesheet.Id == p_id);
    }

    public async Task AddAsync(Timesheet p_timesheet)
    {
        await m_context.Timesheets.AddAsync(p_timesheet);
    }

    public async Task AddRangeAsync(IEnumerable<Timesheet> p_timesheets)
    {
        await m_context.Timesheets.AddRangeAsync(p_timesheets);
    }

    public Task UpdateAsync(Timesheet p_timesheet)
    {
        m_context.Timesheets.Update(p_timesheet);
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(Timesheet p_timesheet)
    {
        p_timesheet.IsDeleted = true;
        m_context.Timesheets.Update(p_timesheet);
        return Task.CompletedTask;
    }

}
