using Crystal.Core.Entities;

namespace Crystal.Core.Interfaces.Repositories;

public interface IJobPositionRepository
{
    Task<IEnumerable<JobPosition>> GetAllAsync();
    Task<JobPosition?> GetByIdAsync(int p_id);
    Task<JobPosition?> GetByNameAsync(string p_name);
    Task AddAsync(JobPosition p_jobPosition);
    Task UpdateAsync(JobPosition p_jobPosition);
    Task SoftDeleteAsync(JobPosition p_jobPosition);
    Task SaveChangesAsync();
}
