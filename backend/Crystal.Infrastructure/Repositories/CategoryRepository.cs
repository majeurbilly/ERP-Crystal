using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Repositories;

public class CategoryRepository : RepositoryBase, ICategoryRepository
{
    public CategoryRepository(CrystalDbContext p_context)
        : base(p_context)
    {
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await m_context.Categories
            .AsNoTracking()
            .OrderBy(p_category => p_category.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int p_id)
    {
        return await m_context.Categories
            .FirstOrDefaultAsync(p_category => p_category.Id == p_id);
    }

    public async Task<Category?> GetByNameAsync(string p_name)
    {
        return await m_context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(p_category => p_category.Name == p_name);
    }

    public async Task AddAsync(Category p_category)
    {
        await m_context.Categories.AddAsync(p_category);
    }

    public void Update(Category p_category)
    {
        m_context.Categories.Update(p_category);
    }

    public void SoftDelete(Category p_category)
    {
        p_category.IsDeleted = true;
        m_context.Categories.Update(p_category);
    }

    public async Task<HashSet<int>> GetExistingActiveIdsAsync(IReadOnlyCollection<int> p_categoryIds)
    {
        if (p_categoryIds.Count == 0)
        {
            return new HashSet<int>();
        }

        List<int> existingIds = await m_context.Categories
            .AsNoTracking()
            .Where(p_category => p_categoryIds.Contains(p_category.Id))
            .Select(p_category => p_category.Id)
            .ToListAsync();

        return existingIds.ToHashSet();
    }
}
