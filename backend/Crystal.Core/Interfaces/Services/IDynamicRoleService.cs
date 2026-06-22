using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface IDynamicRoleService
{
    Task<IEnumerable<DynamicRoleResponseDto>> GetAllAsync();
    Task<DynamicRoleResponseDto?> GetByIdAsync(string p_id);
    Task<DynamicRoleResponseDto> CreateAsync(CreateDynamicRoleRequest p_request);
    Task<DynamicRoleResponseDto> UpdateAsync(string p_id, UpdateDynamicRoleRequest p_request);
    Task DeleteAsync(string p_id);
    IEnumerable<PermissionEntityResponseDto> GetPermissionEntities();
}
