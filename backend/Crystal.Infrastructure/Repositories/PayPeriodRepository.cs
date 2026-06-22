using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Repositories;

public class PayPeriodRepository : RepositoryBase, IPayPeriodRepository
{
    public PayPeriodRepository(CrystalDbContext p_context)
        : base(p_context)
    {
    }

    public async Task<IEnumerable<PayPeriod>> GetAllAsync()
    {
        return await m_context.PayPeriods
            .AsNoTracking()
            .OrderByDescending(p_period => p_period.StartDate)
            .ToListAsync();
    }

    public async Task<PayPeriod?> GetByIdAsync(int p_id)
    {
        return await m_context.PayPeriods
            .AsNoTracking()
            .FirstOrDefaultAsync(p_period => p_period.Id == p_id);
    }

    public async Task AddAsync(PayPeriod p_payPeriod)
    {
        await m_context.PayPeriods.AddAsync(p_payPeriod);
    }

}
