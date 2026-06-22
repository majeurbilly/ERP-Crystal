using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Repositories;

public class InventoryRepository : RepositoryBase, IInventoryRepository
{
    public InventoryRepository(CrystalDbContext p_context)
        : base(p_context)
    {
    }

    public async Task<List<LocationInventoryLineResponseDto>> GetLinesAsync(int? p_locationId, int? p_itemId)
    {
        IQueryable<InventoryLine> query = m_context.InventoryLines.AsNoTracking();

        if (p_locationId.HasValue)
        {
            query = query.Where(p_line => p_line.LocationId == p_locationId.Value);
        }

        if (p_itemId.HasValue)
        {
            query = query.Where(p_line => p_line.ItemId == p_itemId.Value);
        }

        query = query
            .Where(p_line => p_line.Item.IsActive)
            .Where(p_line => p_line.Quantity > 0);

        return await query
            .OrderBy(p_line => p_line.Location.Title)
            .ThenBy(p_line => p_line.Item.Name)
            .Select(p_line => new LocationInventoryLineResponseDto
            {
                LocationId = p_line.LocationId,
                LocationTitle = p_line.Location.Title,
                ItemId = p_line.ItemId,
                ItemName = p_line.Item.Name,
                Quantity = p_line.Quantity
            })
            .ToListAsync();
    }

    public async Task<bool> ItemExistsAsync(int p_itemId)
    {
        return await m_context.Items
            .AnyAsync(p_item => p_item.Id == p_itemId);
    }

    public async Task<bool> IsActiveItemAsync(int p_itemId)
    {
        return await m_context.Items
            .AnyAsync(p_item => p_item.Id == p_itemId && p_item.IsActive);
    }

    public async Task<bool> LocationExistsAsync(int p_locationId)
    {
        return await m_context.Locations
            .AnyAsync(p_location => p_location.Id == p_locationId);
    }

    public async Task<bool> HasInventoryForLocationAsync(int p_locationId)
    {
        return await m_context.InventoryLines
            .AnyAsync(p_line => p_line.LocationId == p_locationId && p_line.Quantity > 0);
    }

    public async Task<InventoryLine?> GetLineByItemAndLocationAsync(int p_itemId, int p_locationId)
    {
        return await m_context.InventoryLines
            .FirstOrDefaultAsync(p_line =>
                p_line.ItemId == p_itemId &&
                p_line.LocationId == p_locationId);
    }

    public async Task<InventoryLine?> GetLineByItemAndLocationReadOnlyAsync(int p_itemId, int p_locationId)
    {
        return await m_context.InventoryLines
            .AsNoTracking()
            .FirstOrDefaultAsync(p_line =>
                p_line.ItemId == p_itemId &&
                p_line.LocationId == p_locationId);
    }

    public void AddLine(InventoryLine p_line)
    {
        m_context.InventoryLines.Add(p_line);
    }

}
