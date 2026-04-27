using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly CrystalDbContext m_context;

    public CrystalDbContext Context => m_context;

    public ItemRepository(CrystalDbContext context)
    {
        m_context = context;
    }

    public async Task<IEnumerable<Item>> GetAllAsync()
    {
        return await Context.Items
            .Include(i => i.InventoryLines)
            .Include(i => i.Book)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Item>> GetAllActiveAsync()
    {
        return await m_context.Items
            .Where(i => i.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }

    public IQueryable<Item> Query()
    {
        return Context.Items
            .Include(i => i.InventoryLines)
            .Include(i => i.Book)
            .AsNoTracking();
    }

    public async Task<Item?> GetByIdAsync(int id)
    {
        return await Context.Items
            .Include(i => i.InventoryLines)
            .Include(i => i.Book)
            .FirstOrDefaultAsync(i => i.Id == id);
    }
}