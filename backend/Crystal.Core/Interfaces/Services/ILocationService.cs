using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface ILocationService
{
    Task<IEnumerable<LocationResponseDto>> GetAllAsync();

    Task<List<LocationOptionResponseDto>> GetDropdownOptionsAsync();

    Task<LocationResponseDto?> GetByIdAsync(int p_id);
    Task<LocationResponseDto> CreateAsync(CreateLocationRequestDto p_request);
    Task<LocationResponseDto> UpdateAsync(int p_id, UpdateLocationRequestDto p_request);
    Task DeleteAsync(int p_id);
}
