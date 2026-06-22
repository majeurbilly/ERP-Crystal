using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Repositories;

public class DynamicRoleRepository : RepositoryBase, IDynamicRoleRepository
{
    public DynamicRoleRepository(CrystalDbContext p_context)
        : base(p_context)
    {
    }

    public async Task<IEnumerable<DynamicRole>> GetAllAsync()
    {
        return await m_context.DynamicRoles
            .AsNoTracking()
            .Include(p_role => p_role.Permissions)
                .ThenInclude(p_permission => p_permission.ScopedLocations)
            .OrderBy(p_role => p_role.Name)
            .ToListAsync();
    }

    public async Task<DynamicRole?> GetByIdAsync(string p_id)
    {
        return await m_context.DynamicRoles
            .FirstOrDefaultAsync(p_role => p_role.Id == p_id);
    }

    public async Task<DynamicRole?> GetByIdWithPermissionsAsync(string p_id)
    {
        return await m_context.DynamicRoles
            .Include(p_role => p_role.Permissions)
                .ThenInclude(p_permission => p_permission.ScopedLocations)
            .FirstOrDefaultAsync(p_role => p_role.Id == p_id);
    }

    public async Task<bool> ExistsAsync(string p_id)
    {
        return await m_context.DynamicRoles
            .AsNoTracking()
            .AnyAsync(p_role => p_role.Id == p_id);
    }

    public async Task<int> CountUsersAssignedAsync(string p_roleId)
    {
        return await m_context.Users
            .AsNoTracking()
            .CountAsync(p_user => p_user.DynamicRoleId == p_roleId);
    }

    public async Task AddAsync(DynamicRole p_role)
    {
        await m_context.DynamicRoles.AddAsync(p_role);
    }

    public Task UpdateAsync(DynamicRole p_role)
    {
        m_context.DynamicRoles.Update(p_role);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(DynamicRole p_role)
    {
        m_context.DynamicRoles.Remove(p_role);
        return Task.CompletedTask;
    }

}
