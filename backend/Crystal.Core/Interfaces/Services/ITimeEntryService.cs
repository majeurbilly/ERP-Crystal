using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface ITimeEntryService
{
    Task<IEnumerable<TimeEntryResponseDto>> GetAllAsync(string p_userId);
    Task<TimeEntryResponseDto?> GetByIdAsync(int p_id, string p_userId);
    Task<TimeEntryResponseDto> CreateAsync(CreateTimeEntryRequest p_request, string p_userId);
    Task<TimeEntryResponseDto> UpdateAsync(int p_id, UpdateTimeEntryRequest p_request);
    Task DeleteAsync(int p_id);
    Task<TimeEntryResponseDto?> GetActiveAsync(string p_userId);
    Task<TimeEntryResponseDto> PunchInAsync(string p_userId);
    Task<TimeEntryResponseDto> PunchOutAsync(string p_userId);
}
