using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Repositories;

public class ItemRepository : RepositoryBase, IItemRepository
{
    public ItemRepository(CrystalDbContext p_context)
        : base(p_context)
    {
    }

    public IQueryable<Item> QueryActiveReadOnly()
    {
        return m_context.Items
            .Include(p_item => p_item.InventoryLines)
            .Include(p_item => p_item.Book)
                .ThenInclude(p_book => p_book!.AuthorBooks)
                .ThenInclude(p_authorBook => p_authorBook.Author)
            .Include(p_item => p_item.Book)
                .ThenInclude(p_book => p_book!.BookCategories)
                .ThenInclude(p_bookCategory => p_bookCategory.Category)
            .Include(p_item => p_item.Book)
                .ThenInclude(p_book => p_book!.BookPublishers)
                .ThenInclude(p_bookPublisher => p_bookPublisher.Publisher)
            .Where(p_item => p_item.IsActive)
            .AsNoTracking();
    }

    public async Task<Item?> GetByIdReadOnlyAsync(int p_id)
    {
        return await m_context.Items
            .Include(p_item => p_item.InventoryLines)
            .Include(p_item => p_item.Book)
                .ThenInclude(p_book => p_book!.AuthorBooks)
                .ThenInclude(p_authorBook => p_authorBook.Author)
            .Include(p_item => p_item.Book)
                .ThenInclude(p_book => p_book!.BookCategories)
                .ThenInclude(p_bookCategory => p_bookCategory.Category)
            .Include(p_item => p_item.Book)
                .ThenInclude(p_book => p_book!.BookPublishers)
                .ThenInclude(p_bookPublisher => p_bookPublisher.Publisher)
            .AsNoTracking()
            .FirstOrDefaultAsync(p_item => p_item.Id == p_id);
    }

    public async Task<Item?> GetByIdAsync(int p_id)
    {
        return await m_context.Items
            .Include(p_item => p_item.InventoryLines)
            .Include(p_item => p_item.Book)
                .ThenInclude(p_book => p_book!.AuthorBooks)
                .ThenInclude(p_authorBook => p_authorBook.Author)
            .Include(p_item => p_item.Book)
                .ThenInclude(p_book => p_book!.BookCategories)
                .ThenInclude(p_bookCategory => p_bookCategory.Category)
            .Include(p_item => p_item.Book)
                .ThenInclude(p_book => p_book!.BookPublishers)
                .ThenInclude(p_bookPublisher => p_bookPublisher.Publisher)
            .FirstOrDefaultAsync(p_item => p_item.Id == p_id);
    }

    public async Task<Item?> GetByIdForUpdateAsync(int p_id)
    {
        return await m_context.Items
            .Include(p_item => p_item.Book)
            .FirstOrDefaultAsync(p_item => p_item.Id == p_id);
    }

    public async Task LoadBookCategoriesAsync(Book p_book)
    {
        await m_context.Entry(p_book)
            .Collection(p_bookEntity => p_bookEntity.BookCategories)
            .LoadAsync();
    }

    public async Task LoadBookAuthorsAsync(Book p_book)
    {
        await m_context.Entry(p_book)
            .Collection(p_bookEntity => p_bookEntity.AuthorBooks)
            .LoadAsync();
    }

    public async Task LoadBookPublishersAsync(Book p_book)
    {
        await m_context.Entry(p_book)
            .Collection(p_bookEntity => p_bookEntity.BookPublishers)
            .LoadAsync();
    }

    public async Task<Item> AddAsync(Item p_item)
    {
        m_context.Items.Add(p_item);
        await m_context.SaveChangesAsync();

        return p_item;
    }

    public async Task<bool> ExistsByNameAsync(string p_name)
    {
        return await m_context.Items
            .AnyAsync(p_item => p_item.IsActive && p_item.Name.ToLower() == p_name.ToLower());
    }

    public async Task RemoveInventoryLinesAsync(int p_itemId)
    {
        List<InventoryLine> lines = await m_context.InventoryLines
            .Where(p_line => p_line.ItemId == p_itemId)
            .ToListAsync();

        m_context.InventoryLines.RemoveRange(lines);
    }

}
