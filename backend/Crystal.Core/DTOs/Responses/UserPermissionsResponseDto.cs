namespace Crystal.Core.DTOs.Responses;

public class UserPermissionsResponseDto
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public IList<PermissionRuleDto> Permissions { get; set; } = new List<PermissionRuleDto>();
}
