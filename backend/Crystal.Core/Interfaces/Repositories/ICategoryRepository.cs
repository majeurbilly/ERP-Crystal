using Crystal.Core.Entities;

namespace Crystal.Core.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int p_id);
    Task<Category?> GetByNameAsync(string p_name);
    Task AddAsync(Category p_category);
    void Update(Category p_category);
    void SoftDelete(Category p_category);

    Task<HashSet<int>> GetExistingActiveIdsAsync(IReadOnlyCollection<int> p_categoryIds);

    Task SaveChangesAsync();
}
