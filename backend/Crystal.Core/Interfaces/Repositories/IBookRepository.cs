using Crystal.Core.Entities;

namespace Crystal.Core.Interfaces.Repositories;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(int p_id);

    Task<Book?> GetByIdForUpdateAsync(int p_id);

    Task<HashSet<int>> GetExistingAuthorIdsAsync(IReadOnlyCollection<int> p_authorIds);

    Task<HashSet<int>> GetExistingCategoryIdsAsync(IReadOnlyCollection<int> p_categoryIds);

    Task<HashSet<int>> GetExistingPublisherIdsAsync(IReadOnlyCollection<int> p_publisherIds);

    Task<List<int>> ResolveAuthorIdsByNamesAsync(IReadOnlyCollection<string> p_names);

    Task<List<int>> ResolvePublisherIdsByNamesAsync(IReadOnlyCollection<string> p_names);

    Task SaveChangesAsync();
}