using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface IAuthorService
{
    Task<IEnumerable<AuthorResponseDto>> GetAllAsync();
    Task<AuthorResponseDto?> GetByIdAsync(int p_id);
    Task<AuthorResponseDto> CreateAsync(CreateAuthorRequest p_request);
    Task<AuthorResponseDto> UpdateAsync(int p_id, UpdateAuthorRequest p_request);
    Task DeleteAsync(int p_id);
}
