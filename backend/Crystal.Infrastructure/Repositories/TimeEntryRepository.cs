using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Repositories;

public class TimeEntryRepository : RepositoryBase, ITimeEntryRepository
{
    public TimeEntryRepository(CrystalDbContext p_context)
        : base(p_context)
    {
    }

    public async Task<IEnumerable<TimeEntry>> GetAllAsync()
    {
        return await m_context.TimeEntries
            .AsNoTracking()
            .Include(p_entry => p_entry.EmployeeProfile)
            .Include(p_entry => p_entry.ScheduledShift)
            .OrderByDescending(p_entry => p_entry.Date)
            .ThenByDescending(p_entry => p_entry.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeEntry>> GetByEmployeeProfileIdAsync(int p_employeeProfileId)
    {
        return await m_context.TimeEntries
            .AsNoTracking()
            .Include(p_entry => p_entry.EmployeeProfile)
            .Include(p_entry => p_entry.ScheduledShift)
            .Where(p_entry => p_entry.EmployeeProfileId == p_employeeProfileId)
            .OrderByDescending(p_entry => p_entry.Date)
            .ThenByDescending(p_entry => p_entry.StartTime)
            .ToListAsync();
    }

    public async Task<TimeEntry?> GetActiveOpenByEmployeeProfileIdAsync(int p_employeeProfileId)
    {
        return await m_context.TimeEntries
            .AsNoTracking()
            .Include(p_entry => p_entry.EmployeeProfile)
            .Include(p_entry => p_entry.ScheduledShift)
            .Where(p_entry => p_entry.EmployeeProfileId == p_employeeProfileId && p_entry.EndTime == null)
            .OrderByDescending(p_entry => p_entry.Date)
            .ThenByDescending(p_entry => p_entry.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task<TimeEntry?> GetTrackedActiveOpenByEmployeeProfileIdAsync(int p_employeeProfileId)
    {
        return await m_context.TimeEntries
            .Where(p_entry => p_entry.EmployeeProfileId == p_employeeProfileId && p_entry.EndTime == null)
            .OrderByDescending(p_entry => p_entry.Date)
            .ThenByDescending(p_entry => p_entry.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task<TimeEntry?> GetByIdAsync(int p_id)
    {
        return await m_context.TimeEntries
            .AsNoTracking()
            .Include(p_entry => p_entry.EmployeeProfile)
            .Include(p_entry => p_entry.ScheduledShift)
            .FirstOrDefaultAsync(p_entry => p_entry.Id == p_id);
    }

    public async Task<TimeEntry?> GetTrackedByIdAsync(int p_id)
    {
        return await m_context.TimeEntries
            .FirstOrDefaultAsync(p_entry => p_entry.Id == p_id);
    }

    public async Task<IList<TimeEntry>> GetTrackedByIdsAsync(IList<int> p_ids)
    {
        if (p_ids.Count == 0)
        {
            return new List<TimeEntry>();
        }

        List<TimeEntry> entries = await m_context.TimeEntries
            .Where(p_entry => p_ids.Contains(p_entry.Id))
            .ToListAsync();

        return entries;
    }

    public async Task<IList<TimeEntry>> GetTrackedUnlinkedByPeriodAsync(DateOnly p_periodStart, DateOnly p_periodEnd)
    {
        return await m_context.TimeEntries
            .Where(p_entry =>
                !p_entry.TimesheetId.HasValue
                && p_entry.Date >= p_periodStart
                && p_entry.Date <= p_periodEnd)
            .ToListAsync();
    }

    public async Task<IList<TimeEntry>> GetTrackedByTimesheetIdAsync(int p_timesheetId)
    {
        List<TimeEntry> entries = await m_context.TimeEntries
            .Where(p_entry => p_entry.TimesheetId == p_timesheetId)
            .ToListAsync();

        return entries;
    }

    public async Task AddAsync(TimeEntry p_timeEntry)
    {
        await m_context.TimeEntries.AddAsync(p_timeEntry);
    }

    public Task UpdateAsync(TimeEntry p_timeEntry)
    {
        m_context.TimeEntries.Update(p_timeEntry);
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(TimeEntry p_timeEntry)
    {
        p_timeEntry.IsDeleted = true;
        m_context.TimeEntries.Update(p_timeEntry);
        return Task.CompletedTask;
    }

}
