using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface IBookService
{
    Task<BookResponseDto?> GetByIdAsync(int p_id);

    Task<BookResponseDto?> UpdateBookRelationsAsync(int p_id, UpdateBookRequest p_request);
}