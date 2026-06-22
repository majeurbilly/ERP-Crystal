using Crystal.Core.Entities;

namespace Crystal.Core.Interfaces.Repositories;

public interface IEmployeeProfileRepository
{
    Task<int> CountActiveAsync();
    Task<IEnumerable<EmployeeProfile>> GetAllAsync();
    Task<EmployeeProfile?> GetByIdAsync(int p_id);
    Task<EmployeeProfile?> GetTrackedByIdAsync(int p_id);
    Task<EmployeeProfile?> GetByApplicationUserIdAsync(string p_applicationUserId);
    Task<bool> IsEmailUniqueAsync(string p_email, int? p_excludeId = null);
    Task<bool> IsApplicationUserIdAvailableAsync(string p_applicationUserId, int? p_excludeId = null);
    Task AddAsync(EmployeeProfile p_employeeProfile);
    Task UpdateAsync(EmployeeProfile p_employeeProfile);
    Task SoftDeleteAsync(EmployeeProfile p_employeeProfile);
    Task SaveChangesAsync();
}
