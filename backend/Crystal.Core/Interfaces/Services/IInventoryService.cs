using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface IInventoryService
{
    Task<IEnumerable<LocationInventoryLineResponseDto>> GetInventoryLinesAsync(int? p_locationId, int? p_itemId);

    Task UpdateQuantityAsync(UpdateInventoryQuantityRequest p_request);
    Task AddQuantityAsync(UpdateInventoryQuantityRequest p_request);

    Task<InventoryStockResponseDto> GetStockAsync(int p_locationId, int p_itemId);

    Task SetStockAsync(int p_locationId, int p_itemId, UpdateStockRequest p_request);

    Task ImportFromExcelAsync(Stream p_fileStream, string p_fileName);
}