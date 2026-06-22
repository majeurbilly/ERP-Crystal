using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Repositories;

public class PayStubRepository : RepositoryBase, IPayStubRepository
{
    public PayStubRepository(CrystalDbContext p_context)
        : base(p_context)
    {
    }

    public async Task<decimal> SumGrossPayAsync()
    {
        bool hasPayStubs = await m_context.PayStubs.AsNoTracking().AnyAsync();

        if (!hasPayStubs)
        {
            return 0m;
        }

        return await m_context.PayStubs
            .AsNoTracking()
            .SumAsync(p_stub => p_stub.GrossPay);
    }

    public async Task<IEnumerable<PayStub>> GetAllAsync()
    {
        return await m_context.PayStubs
            .AsNoTracking()
            .Include(p_stub => p_stub.EmployeeProfile)
            .Include(p_stub => p_stub.PayPeriod)
            .OrderByDescending(p_stub => p_stub.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<PayStub>> GetPublishedByEmployeeProfileIdAsync(int p_employeeProfileId)
    {
        return await m_context.PayStubs
            .AsNoTracking()
            .Include(p_stub => p_stub.EmployeeProfile)
            .Include(p_stub => p_stub.PayPeriod)
            .Where(p_stub =>
                p_stub.EmployeeProfileId == p_employeeProfileId
                && p_stub.IsPublished)
            .OrderByDescending(p_stub => p_stub.Id)
            .ToListAsync();
    }

    public async Task<IList<PayStub>> GetByPayPeriodIdAsync(int p_payPeriodId)
    {
        return await m_context.PayStubs
            .AsNoTracking()
            .Include(p_stub => p_stub.EmployeeProfile)
            .Include(p_stub => p_stub.PayPeriod)
            .Where(p_stub => p_stub.PayPeriodId == p_payPeriodId)
            .OrderBy(p_stub => p_stub.EmployeeProfile.LastName)
            .ThenBy(p_stub => p_stub.EmployeeProfile.FirstName)
            .ToListAsync();
    }

    public async Task<PayStub?> GetByIdAsync(int p_id)
    {
        return await m_context.PayStubs
            .AsNoTracking()
            .Include(p_stub => p_stub.EmployeeProfile)
            .Include(p_stub => p_stub.PayPeriod)
            .FirstOrDefaultAsync(p_stub => p_stub.Id == p_id);
    }

    public async Task<PayStub?> GetTrackedByIdAsync(int p_id)
    {
        return await m_context.PayStubs
            .Include(p_stub => p_stub.PayPeriod)
            .Include(p_stub => p_stub.Timesheet)
            .FirstOrDefaultAsync(p_stub => p_stub.Id == p_id);
    }

    public async Task AddAsync(PayStub p_payStub)
    {
        await m_context.PayStubs.AddAsync(p_payStub);
    }

    public async Task AddRangeAsync(IEnumerable<PayStub> p_payStubs)
    {
        await m_context.PayStubs.AddRangeAsync(p_payStubs);
    }

}
