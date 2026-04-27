using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface IItemService
{
    Task<IEnumerable<ItemResponse>> GetAllItemsAsync(CancellationToken p_cancellationToken = default);
    Task<ItemResponse?> GetItemByIdAsync(int p_id, CancellationToken p_cancellationToken = default);
    Task<ItemResponse> CreateItemAsync(CreateItemRequest p_request, CancellationToken p_cancellationToken = default);
    Task<ItemResponse?> UpdateItemAsync(int p_id, UpdateItemRequest p_request, CancellationToken p_cancellationToken = default);
    Task<bool> DeleteItemAsync(int p_id, CancellationToken p_cancellationToken = default);
}