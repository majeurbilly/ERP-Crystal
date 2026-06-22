using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Repositories;

public class EmploymentContractRepository : RepositoryBase, IEmploymentContractRepository
{
    public EmploymentContractRepository(CrystalDbContext p_context)
        : base(p_context)
    {
    }

    public async Task<IEnumerable<EmploymentContract>> GetAllAsync()
    {
        return await m_context.EmploymentContracts
            .AsNoTracking()
            .Include(p_contract => p_contract.EmployeeProfile)
            .OrderByDescending(p_contract => p_contract.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmploymentContract>> GetByEmployeeProfileIdAsync(int p_employeeProfileId)
    {
        return await m_context.EmploymentContracts
            .AsNoTracking()
            .Include(p_contract => p_contract.EmployeeProfile)
            .Where(p_contract => p_contract.EmployeeProfileId == p_employeeProfileId)
            .OrderByDescending(p_contract => p_contract.StartDate)
            .ToListAsync();
    }

    public async Task<EmploymentContract?> GetByIdAsync(int p_id)
    {
        return await m_context.EmploymentContracts
            .AsNoTracking()
            .Include(p_contract => p_contract.EmployeeProfile)
            .FirstOrDefaultAsync(p_contract => p_contract.Id == p_id);
    }

    public async Task<EmploymentContract?> GetActiveForEmployeeAndPeriodAsync(
        int p_employeeProfileId,
        DateOnly p_periodStart,
        DateOnly p_periodEnd)
    {
        return await m_context.EmploymentContracts
            .AsNoTracking()
            .Where(p_contract => p_contract.EmployeeProfileId == p_employeeProfileId)
            .Where(p_contract =>
                p_contract.StartDate <= p_periodEnd
                && (p_contract.EndDate == null || p_contract.EndDate >= p_periodStart))
            .OrderByDescending(p_contract => p_contract.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<EmploymentContract?> GetTrackedByIdAsync(int p_id)
    {
        return await m_context.EmploymentContracts
            .FirstOrDefaultAsync(p_contract => p_contract.Id == p_id);
    }

    public async Task<bool> HasOverlappingContractsAsync(
        int p_employeeProfileId,
        DateOnly p_startDate,
        DateOnly? p_endDate,
        int? p_excludeContractId = null)
    {
        DateOnly effectiveEndDate = p_endDate ?? DateOnly.MaxValue;

        bool hasOverlap = await m_context.EmploymentContracts
            .AsNoTracking()
            .Where(p_contract => p_contract.EmployeeProfileId == p_employeeProfileId)
            .Where(p_contract => !p_excludeContractId.HasValue || p_contract.Id != p_excludeContractId.Value)
            .AnyAsync(p_contract =>
                p_startDate <= (p_contract.EndDate ?? DateOnly.MaxValue)
                && p_contract.StartDate <= effectiveEndDate);

        return hasOverlap;
    }

    public async Task AddAsync(EmploymentContract p_contract)
    {
        await m_context.EmploymentContracts.AddAsync(p_contract);
    }

    public Task UpdateAsync(EmploymentContract p_contract)
    {
        m_context.EmploymentContracts.Update(p_contract);
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(EmploymentContract p_contract)
    {
        p_contract.IsDeleted = true;
        m_context.EmploymentContracts.Update(p_contract);
        return Task.CompletedTask;
    }

}
