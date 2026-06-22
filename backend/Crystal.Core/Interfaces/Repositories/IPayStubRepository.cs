using Crystal.Core.Entities;

namespace Crystal.Core.Interfaces.Repositories;

public interface IPayStubRepository
{
    Task<decimal> SumGrossPayAsync();
    Task<IEnumerable<PayStub>> GetAllAsync();
    Task<IEnumerable<PayStub>> GetPublishedByEmployeeProfileIdAsync(int p_employeeProfileId);
    Task<IList<PayStub>> GetByPayPeriodIdAsync(int p_payPeriodId);
    Task<PayStub?> GetByIdAsync(int p_id);
    Task<PayStub?> GetTrackedByIdAsync(int p_id);
    Task AddAsync(PayStub p_payStub);
    Task AddRangeAsync(IEnumerable<PayStub> p_payStubs);
    Task SaveChangesAsync();
}
