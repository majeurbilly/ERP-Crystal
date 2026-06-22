using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface IEmployeeProfileService
{
    Task<IEnumerable<EmployeeProfileResponseDto>> GetAllAsync(string p_userId);
    Task<EmployeeProfileResponseDto?> GetByIdAsync(int p_id, string p_userId);
    Task<EmployeeProfileResponseDto> CreateAsync(CreateEmployeeProfileRequest p_request);
    Task<EmployeeProfileResponseDto> UpdateAsync(int p_id, UpdateEmployeeProfileRequest p_request);
    Task DeleteAsync(int p_id);
    Task<EmployeeProfileResponseDto> GetMyProfileAsync(string p_applicationUserId);
}
