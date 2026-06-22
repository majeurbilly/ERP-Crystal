using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;

namespace Crystal.Core.Interfaces.Repositories;

public interface IInventoryRepository
{
    Task<List<LocationInventoryLineResponseDto>> GetLinesAsync(int? p_locationId, int? p_itemId);
    Task<bool> ItemExistsAsync(int p_itemId);
    Task<bool> IsActiveItemAsync(int p_itemId);
    Task<bool> LocationExistsAsync(int p_locationId);
    Task<bool> HasInventoryForLocationAsync(int p_locationId);
    Task<InventoryLine?> GetLineByItemAndLocationAsync(int p_itemId, int p_locationId);

    Task<InventoryLine?> GetLineByItemAndLocationReadOnlyAsync(int p_itemId, int p_locationId);
    void AddLine(InventoryLine p_line);
    Task SaveChangesAsync();
}
