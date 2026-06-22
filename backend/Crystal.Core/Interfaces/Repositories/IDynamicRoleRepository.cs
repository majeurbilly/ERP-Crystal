using Crystal.Core.Entities;

namespace Crystal.Core.Interfaces.Repositories;

public interface IDynamicRoleRepository
{
    Task<IEnumerable<DynamicRole>> GetAllAsync();
    Task<DynamicRole?> GetByIdAsync(string p_id);
    Task<DynamicRole?> GetByIdWithPermissionsAsync(string p_id);
    Task<bool> ExistsAsync(string p_id);
    Task<int> CountUsersAssignedAsync(string p_roleId);
    Task AddAsync(DynamicRole p_role);
    Task UpdateAsync(DynamicRole p_role);
    Task DeleteAsync(DynamicRole p_role);
    Task SaveChangesAsync();
}
