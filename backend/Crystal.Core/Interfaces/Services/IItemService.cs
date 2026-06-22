using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface IItemService
{
    Task<IEnumerable<ItemResponseDto>> GetInventoryAsync(
        string? p_search = null,
        int? p_publisherId = null,
        int[]? p_categoryIds = null,
        int? p_authorId = null,
        bool? p_isBook = null);

    Task<ItemResponseDto?> GetByIdAsync(int p_id);
    Task<ItemResponseDto> CreateAsync(CreateItemRequest p_request);
    Task<ItemResponseDto> CreateBookAsync(CreateBookRequest p_request);
    Task<ItemResponseDto?> UpdateAsync(int p_id, UpdateItemRequest p_request);
    Task<bool> DeleteAsync(int p_id);

    Task<ItemResponseDto?> UploadImageAsync(int p_id, Stream p_fileStream, string p_fileName);
}