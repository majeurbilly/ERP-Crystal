using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface IJobPositionService
{
    Task<IEnumerable<JobPositionResponseDto>> GetAllAsync();
    Task<JobPositionResponseDto?> GetByIdAsync(int p_id);
    Task<JobPositionResponseDto> CreateAsync(CreateJobPositionRequest p_request);
    Task<JobPositionResponseDto> UpdateAsync(int p_id, UpdateJobPositionRequest p_request);
    Task DeleteAsync(int p_id);
}
