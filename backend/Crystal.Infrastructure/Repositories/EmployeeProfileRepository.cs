using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Repositories;

public class EmployeeProfileRepository : RepositoryBase, IEmployeeProfileRepository
{
    public EmployeeProfileRepository(CrystalDbContext p_context)
        : base(p_context)
    {
    }

    public async Task<int> CountActiveAsync()
    {
        return await m_context.EmployeeProfiles
            .AsNoTracking()
            .CountAsync();
    }

    public async Task<IEnumerable<EmployeeProfile>> GetAllAsync()
    {
        return await m_context.EmployeeProfiles
            .AsNoTracking()
            .Include(p_profile => p_profile.JobPosition)
            .Include(p_profile => p_profile.Location)
            .OrderBy(p_profile => p_profile.LastName)
            .ThenBy(p_profile => p_profile.FirstName)
            .ToListAsync();
    }

    public async Task<EmployeeProfile?> GetByIdAsync(int p_id)
    {
        return await m_context.EmployeeProfiles
            .AsNoTracking()
            .Include(p_profile => p_profile.JobPosition)
            .Include(p_profile => p_profile.Location)
            .FirstOrDefaultAsync(p_profile => p_profile.Id == p_id);
    }

    public async Task<EmployeeProfile?> GetTrackedByIdAsync(int p_id)
    {
        return await m_context.EmployeeProfiles
            .FirstOrDefaultAsync(p_profile => p_profile.Id == p_id);
    }

    public async Task<EmployeeProfile?> GetByApplicationUserIdAsync(string p_applicationUserId)
    {
        return await m_context.EmployeeProfiles
            .AsNoTracking()
            .Include(p_profile => p_profile.JobPosition)
            .Include(p_profile => p_profile.Location)
            .FirstOrDefaultAsync(p_profile => p_profile.ApplicationUserId == p_applicationUserId);
    }

    public async Task<bool> IsApplicationUserIdAvailableAsync(string p_applicationUserId, int? p_excludeId = null)
    {
        bool applicationUserIdExists = await m_context.EmployeeProfiles
            .AsNoTracking()
            .AnyAsync(p_profile =>
                p_profile.ApplicationUserId == p_applicationUserId
                && (!p_excludeId.HasValue || p_profile.Id != p_excludeId.Value));

        return !applicationUserIdExists;
    }

    public async Task<bool> IsEmailUniqueAsync(string p_email, int? p_excludeId = null)
    {
        bool emailExists = await m_context.EmployeeProfiles
            .AsNoTracking()
            .AnyAsync(p_profile =>
                p_profile.Email == p_email
                && (!p_excludeId.HasValue || p_profile.Id != p_excludeId.Value));

        return !emailExists;
    }

    public async Task AddAsync(EmployeeProfile p_employeeProfile)
    {
        await m_context.EmployeeProfiles.AddAsync(p_employeeProfile);
    }

    public Task UpdateAsync(EmployeeProfile p_employeeProfile)
    {
        m_context.EmployeeProfiles.Update(p_employeeProfile);
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(EmployeeProfile p_employeeProfile)
    {
        p_employeeProfile.IsDeleted = true;
        m_context.EmployeeProfiles.Update(p_employeeProfile);
        return Task.CompletedTask;
    }

}
