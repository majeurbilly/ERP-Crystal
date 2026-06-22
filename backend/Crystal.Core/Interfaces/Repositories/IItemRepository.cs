using Crystal.Core.Entities;

namespace Crystal.Core.Interfaces.Repositories;

public interface IItemRepository
{
    IQueryable<Item> QueryActiveReadOnly();

    Task<Item?> GetByIdAsync(int p_id);

    Task<Item?> GetByIdForUpdateAsync(int p_id);

    Task LoadBookCategoriesAsync(Book p_book);

    Task LoadBookAuthorsAsync(Book p_book);

    Task LoadBookPublishersAsync(Book p_book);

    Task<Item?> GetByIdReadOnlyAsync(int p_id);
    Task<Item> AddAsync(Item p_item);
    Task<bool> ExistsByNameAsync(string p_name);
    Task RemoveInventoryLinesAsync(int p_itemId);
    Task SaveChangesAsync();
}