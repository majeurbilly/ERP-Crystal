using Crystal.Core.Entities;

namespace Crystal.Core.Interfaces.Repositories;

public interface IPayPeriodRepository
{
    Task<IEnumerable<PayPeriod>> GetAllAsync();
    Task<PayPeriod?> GetByIdAsync(int p_id);
    Task AddAsync(PayPeriod p_payPeriod);
    Task SaveChangesAsync();
}
