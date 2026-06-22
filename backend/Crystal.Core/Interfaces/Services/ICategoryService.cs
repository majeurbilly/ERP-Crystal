using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponseDto>> GetAllAsync();
    Task<CategoryResponseDto?> GetByIdAsync(int p_id);
    Task<CategoryResponseDto> CreateAsync(CreateCategoryRequestDto p_request);
    Task<CategoryResponseDto> UpdateAsync(int p_id, UpdateCategoryRequestDto p_request);
    Task DeleteAsync(int p_id);
}
