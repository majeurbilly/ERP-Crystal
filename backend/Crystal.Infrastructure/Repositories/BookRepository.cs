using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Repositories;

public class BookRepository : RepositoryBase, IBookRepository
{
    public BookRepository(CrystalDbContext p_context)
        : base(p_context)
    {
    }

    public async Task<Book?> GetByIdAsync(int p_id)
    {
        return await m_context.Books
            .AsNoTracking()
            .Include(p_book => p_book.Item)
                .ThenInclude(p_item => p_item.InventoryLines)
            .Include(p_book => p_book.AuthorBooks)
                .ThenInclude(p_authorBook => p_authorBook.Author)
            .Include(p_book => p_book.BookCategories)
                .ThenInclude(p_bookCategory => p_bookCategory.Category)
            .Include(p_book => p_book.BookPublishers)
                .ThenInclude(p_bookPublisher => p_bookPublisher.Publisher)
            .FirstOrDefaultAsync(p_book => p_book.ItemId == p_id);
    }

    public async Task<Book?> GetByIdForUpdateAsync(int p_id)
    {
        return await m_context.Books
            .Include(p_book => p_book.AuthorBooks)
            .Include(p_book => p_book.BookCategories)
            .Include(p_book => p_book.BookPublishers)
            .FirstOrDefaultAsync(p_book => p_book.ItemId == p_id);
    }

    public async Task<HashSet<int>> GetExistingAuthorIdsAsync(IReadOnlyCollection<int> p_authorIds)
    {
        if (p_authorIds.Count == 0)
        {
            return new HashSet<int>();
        }

        List<int> existingIds = await m_context.Authors
            .AsNoTracking()
            .Where(p_author => p_authorIds.Contains(p_author.Id))
            .Select(p_author => p_author.Id)
            .ToListAsync();

        return existingIds.ToHashSet();
    }

    public async Task<HashSet<int>> GetExistingCategoryIdsAsync(IReadOnlyCollection<int> p_categoryIds)
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

    public async Task<HashSet<int>> GetExistingPublisherIdsAsync(IReadOnlyCollection<int> p_publisherIds)
    {
        if (p_publisherIds.Count == 0)
        {
            return new HashSet<int>();
        }

        List<int> existingIds = await m_context.Publishers
            .AsNoTracking()
            .Where(p_publisher => p_publisherIds.Contains(p_publisher.Id))
            .Select(p_publisher => p_publisher.Id)
            .ToListAsync();

        return existingIds.ToHashSet();
    }

    public async Task<List<int>> ResolveAuthorIdsByNamesAsync(IReadOnlyCollection<string> p_names)
    {
        List<string> distinctNames = NormalizeDistinctNames(p_names);

        if (distinctNames.Count == 0)
        {
            return new List<int>();
        }

        List<Author> resolvedAuthors = new();

        foreach (string name in distinctNames)
        {
            Author? author = await m_context.Authors
                .FirstOrDefaultAsync(p_author => p_author.Name.ToLower() == name.ToLower());

            if (author is null)
            {
                author = new Author { Name = name };
                m_context.Authors.Add(author);
            }

            resolvedAuthors.Add(author);
        }

        await m_context.SaveChangesAsync();

        return resolvedAuthors.Select(p_author => p_author.Id).ToList();
    }

    public async Task<List<int>> ResolvePublisherIdsByNamesAsync(IReadOnlyCollection<string> p_names)
    {
        List<string> distinctNames = NormalizeDistinctNames(p_names);

        if (distinctNames.Count == 0)
        {
            return new List<int>();
        }

        List<Publisher> resolvedPublishers = new();

        foreach (string name in distinctNames)
        {
            Publisher? publisher = await m_context.Publishers
                .FirstOrDefaultAsync(p_publisher => p_publisher.Name.ToLower() == name.ToLower());

            if (publisher is null)
            {
                publisher = new Publisher { Name = name };
                m_context.Publishers.Add(publisher);
            }

            resolvedPublishers.Add(publisher);
        }

        await m_context.SaveChangesAsync();

        return resolvedPublishers.Select(p_publisher => p_publisher.Id).ToList();
    }

    private static List<string> NormalizeDistinctNames(IReadOnlyCollection<string> p_names)
    {
        return p_names
            .Select(p_name => p_name.Trim())
            .Where(p_name => !string.IsNullOrWhiteSpace(p_name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
