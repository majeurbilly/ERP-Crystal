using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Services;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Services;

public class ItemService : IItemService
{
    private readonly CrystalDbContext m_dbContext;

    public ItemService(CrystalDbContext p_dbContext)
    {
        m_dbContext = p_dbContext;
    }

    public async Task<IEnumerable<ItemResponse>> GetAllItemsAsync(CancellationToken p_cancellationToken = default)
    {
        // Charge les relations nécessaires pour l'affichage de l'inventaire.
        List<Item> items = await m_dbContext.Items
            .Include(i => i.Book)
            .Include(i => i.InventoryLines)
            .Where(i => i.IsActive)
            .ToListAsync(p_cancellationToken);

        List<ItemResponse> responses = items.Select(i => new ItemResponse
        {
            Id = i.Id,
            Name = i.Name,
            Description = i.Description,
            Price = i.Price,
            AlertQuantity = i.AlertQuantity,
            TotalQuantity = i.InventoryLines.Sum(l => l.Quantity),
            LastUpdate = i.LastUpdate,
            HasBook = i.Book != null
        }).ToList();

        return responses;
    }

    public async Task<ItemResponse?> GetItemByIdAsync(int p_id, CancellationToken p_cancellationToken = default)
    {
        // Charge l'item actif avec ses relations pour l'écran de details.
        Item? item = await m_dbContext.Items
            .Include(i => i.Book)
            .Include(i => i.InventoryLines)
            .Where(i => i.IsActive)
            .FirstOrDefaultAsync(i => i.Id == p_id, p_cancellationToken);

        if (item == null)
        {
            return null;
        }

        ItemResponse response = new ItemResponse
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            AlertQuantity = item.AlertQuantity,
            TotalQuantity = item.InventoryLines.Sum(l => l.Quantity),
            LastUpdate = item.LastUpdate,
            HasBook = item.Book != null
        };

        return response;
    }

    public async Task<ItemResponse> CreateItemAsync(CreateItemRequest p_request, CancellationToken p_cancellationToken = default)
    {
        Item item = new Item
        {
            Name = p_request.Name,
            Description = p_request.Description,
            Price = p_request.Price,
            AlertQuantity = p_request.AlertQuantity,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        m_dbContext.Items.Add(item);

        if (p_request.InitialQuantity != 0)
        {
            // Selectionne une localisation existante pour la ligne de stock initial.
            int? locationId = await m_dbContext.Locations
                .Select(l => (int?)l.Id)
                .FirstOrDefaultAsync(p_cancellationToken);

            if (locationId == null)
            {
                throw new InvalidOperationException("No location available to create initial stock line.");
            }

            InventoryLine inventoryLine = new InventoryLine
            {
                Item = item,
                LocationId = locationId.Value,
                Quantity = p_request.InitialQuantity
            };

            m_dbContext.InventoryLines.Add(inventoryLine);
        }

        await m_dbContext.SaveChangesAsync(p_cancellationToken);

        ItemResponse response = new ItemResponse
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            AlertQuantity = item.AlertQuantity,
            TotalQuantity = p_request.InitialQuantity,
            LastUpdate = item.LastUpdate,
            HasBook = item.Book != null
        };

        return response;
    }

    public async Task<ItemResponse?> UpdateItemAsync(int p_id, UpdateItemRequest p_request, CancellationToken p_cancellationToken = default)
    {
        Item? item = await m_dbContext.Items
            .Include(i => i.Book)
            .Include(i => i.InventoryLines)
            .Where(i => i.IsActive)
            .FirstOrDefaultAsync(i => i.Id == p_id, p_cancellationToken);

        if (item == null)
        {
            return null;
        }

        item.Name = p_request.Name;
        item.Description = p_request.Description;
        item.Price = p_request.Price;
        item.AlertQuantity = p_request.AlertQuantity;
        item.LastUpdate = DateTime.UtcNow;

        if (p_request.BookId.HasValue)
        {
            Book? book = await m_dbContext.Books
                .FirstOrDefaultAsync(b => b.Id == p_request.BookId.Value && b.ItemId == item.Id, p_cancellationToken);

            item.Book = book;
        }
        else
        {
            item.Book = null;
        }

        await m_dbContext.SaveChangesAsync(p_cancellationToken);

        ItemResponse response = new ItemResponse
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            AlertQuantity = item.AlertQuantity,
            TotalQuantity = item.InventoryLines.Sum(l => l.Quantity),
            LastUpdate = item.LastUpdate,
            HasBook = item.Book != null
        };

        return response;
    }

    public async Task<bool> DeleteItemAsync(int p_id, CancellationToken p_cancellationToken = default)
    {
        Item? item = await m_dbContext.Items
            .FirstOrDefaultAsync(i => i.Id == p_id, p_cancellationToken);

        if (item == null || !item.IsActive)
        {
            return false;
        }

        item.IsActive = false;
        item.LastUpdate = DateTime.UtcNow;

        await m_dbContext.SaveChangesAsync(p_cancellationToken);
        return true;
    }
}