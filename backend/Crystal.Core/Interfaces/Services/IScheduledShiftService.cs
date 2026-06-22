using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface IScheduledShiftService
{
    Task<IEnumerable<ScheduledShiftResponseDto>> GetAllAsync(string p_userId);

    Task<IEnumerable<ScheduledShiftResponseDto>> GetTeamScheduleAsync(string p_userId);
    Task<ScheduledShiftResponseDto?> GetByIdAsync(int p_id, string p_userId);
    Task<ScheduledShiftResponseDto> CreateAsync(CreateScheduledShiftRequest p_request);
    Task<ScheduledShiftResponseDto> UpdateAsync(int p_id, UpdateScheduledShiftRequest p_request);
    Task DeleteAsync(int p_id);
}
