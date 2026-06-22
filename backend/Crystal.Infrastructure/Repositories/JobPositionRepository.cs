using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Repositories;

public class JobPositionRepository : RepositoryBase, IJobPositionRepository
{
    public JobPositionRepository(CrystalDbContext p_context)
        : base(p_context)
    {
    }

    public async Task<IEnumerable<JobPosition>> GetAllAsync()
    {
        return await m_context.JobPositions
            .AsNoTracking()
            .OrderBy(p_position => p_position.Name)
            .ToListAsync();
    }

    public async Task<JobPosition?> GetByIdAsync(int p_id)
    {
        return await m_context.JobPositions
            .FirstOrDefaultAsync(p_position => p_position.Id == p_id);
    }

    public async Task<JobPosition?> GetByNameAsync(string p_name)
    {
        return await m_context.JobPositions
            .AsNoTracking()
            .FirstOrDefaultAsync(p_position => p_position.Name == p_name);
    }

    public async Task AddAsync(JobPosition p_jobPosition)
    {
        await m_context.JobPositions.AddAsync(p_jobPosition);
    }

    public Task UpdateAsync(JobPosition p_jobPosition)
    {
        m_context.JobPositions.Update(p_jobPosition);
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(JobPosition p_jobPosition)
    {
        p_jobPosition.IsDeleted = true;
        m_context.JobPositions.Update(p_jobPosition);
        return Task.CompletedTask;
    }
}
