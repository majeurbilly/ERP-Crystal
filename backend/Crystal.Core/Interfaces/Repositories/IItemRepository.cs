using Crystal.Core.Entities;

namespace Crystal.Core.Interfaces.Repositories;

public interface IItemRepository
{
    Task<IEnumerable<Item>> GetAllAsync();

    Task<IEnumerable<Item>> GetAllActiveAsync();

    IQueryable<Item> Query();

    Task<Item?> GetByIdAsync(int p_id);
}