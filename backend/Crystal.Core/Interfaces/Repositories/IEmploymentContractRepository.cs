using Crystal.Core.Entities;

namespace Crystal.Core.Interfaces.Repositories;

public interface IEmploymentContractRepository
{
    Task<IEnumerable<EmploymentContract>> GetAllAsync();
    Task<IEnumerable<EmploymentContract>> GetByEmployeeProfileIdAsync(int p_employeeProfileId);
    Task<EmploymentContract?> GetByIdAsync(int p_id);
    Task<EmploymentContract?> GetActiveForEmployeeAndPeriodAsync(int p_employeeProfileId, DateOnly p_periodStart, DateOnly p_periodEnd);
    Task<EmploymentContract?> GetTrackedByIdAsync(int p_id);
    Task<bool> HasOverlappingContractsAsync(int p_employeeProfileId, DateOnly p_startDate, DateOnly? p_endDate, int? p_excludeContractId = null);
    Task AddAsync(EmploymentContract p_contract);
    Task UpdateAsync(EmploymentContract p_contract);
    Task SoftDeleteAsync(EmploymentContract p_contract);
    Task SaveChangesAsync();
}
