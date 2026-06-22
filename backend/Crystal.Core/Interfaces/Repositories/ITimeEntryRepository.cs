using Crystal.Core.Entities;

namespace Crystal.Core.Interfaces.Repositories;

public interface ITimeEntryRepository
{
    Task<IEnumerable<TimeEntry>> GetAllAsync();
    Task<IEnumerable<TimeEntry>> GetByEmployeeProfileIdAsync(int p_employeeProfileId);
    Task<TimeEntry?> GetActiveOpenByEmployeeProfileIdAsync(int p_employeeProfileId);
    Task<TimeEntry?> GetTrackedActiveOpenByEmployeeProfileIdAsync(int p_employeeProfileId);
    Task<TimeEntry?> GetByIdAsync(int p_id);
    Task<TimeEntry?> GetTrackedByIdAsync(int p_id);
    Task<IList<TimeEntry>> GetTrackedByIdsAsync(IList<int> p_ids);
    Task<IList<TimeEntry>> GetTrackedUnlinkedByPeriodAsync(DateOnly p_periodStart, DateOnly p_periodEnd);
    Task<IList<TimeEntry>> GetTrackedByTimesheetIdAsync(int p_timesheetId);
    Task AddAsync(TimeEntry p_timeEntry);
    Task UpdateAsync(TimeEntry p_timeEntry);
    Task SoftDeleteAsync(TimeEntry p_timeEntry);
    Task SaveChangesAsync();
}
