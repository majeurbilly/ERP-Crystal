using Crystal.Core.Entities;

namespace Crystal.Core.Interfaces.Repositories;

public interface IScheduledShiftRepository
{
    Task<IEnumerable<ScheduledShift>> GetAllAsync();
    Task<IEnumerable<ScheduledShift>> GetByEmployeeProfileIdAsync(int p_employeeProfileId);
    Task<ScheduledShift?> GetByEmployeeProfileIdAndDateAsync(int p_employeeProfileId, DateOnly p_date);
    Task<ScheduledShift?> GetByIdAsync(int p_id);
    Task<ScheduledShift?> GetTrackedByIdAsync(int p_id);
    Task AddAsync(ScheduledShift p_scheduledShift);
    Task UpdateAsync(ScheduledShift p_scheduledShift);
    Task SoftDeleteAsync(ScheduledShift p_scheduledShift);
    Task SaveChangesAsync();
}
