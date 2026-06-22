using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface ILeaveRequestService
{
    Task<IEnumerable<LeaveRequestResponseDto>> GetAllAsync(string p_userId);
    Task<LeaveRequestResponseDto?> GetByIdAsync(int p_id, string p_userId);
    Task<LeaveRequestResponseDto> CreateAsync(CreateLeaveRequestDto p_request, string p_userId);
    Task<LeaveRequestResponseDto> UpdateStatusAsync(int p_id, UpdateLeaveRequestStatusDto p_request);
    Task DeleteAsync(int p_id);
}
