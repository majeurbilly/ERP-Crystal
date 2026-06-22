using Crystal.Core.Entities;
using Crystal.Core.Enums;

namespace Crystal.Core.Interfaces.Repositories;

public interface ITimesheetRepository
{
    Task<int> CountByStatusAsync(TimesheetStatus p_status);
    Task<IEnumerable<Timesheet>> GetAllAsync();
    Task<IEnumerable<Timesheet>> GetByEmployeeProfileIdAsync(int p_employeeProfileId);
    Task<IList<Timesheet>> GetByPeriodAsync(DateOnly p_periodStart, DateOnly p_periodEnd);
    Task<Timesheet?> GetByIdAsync(int p_id);
    Task<Timesheet?> GetApprovedByEmployeeAndPeriodAsync(int p_employeeProfileId, DateOnly p_periodStart, DateOnly p_periodEnd);
    Task<Timesheet?> GetTrackedByIdAsync(int p_id);
    Task AddAsync(Timesheet p_timesheet);
    Task AddRangeAsync(IEnumerable<Timesheet> p_timesheets);
    Task UpdateAsync(Timesheet p_timesheet);
    Task SoftDeleteAsync(Timesheet p_timesheet);
    Task SaveChangesAsync();
}
