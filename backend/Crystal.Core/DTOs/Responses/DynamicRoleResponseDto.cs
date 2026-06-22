namespace Crystal.Core.DTOs.Responses;

public class DynamicRoleResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsPreset { get; set; }
    public IList<PermissionRuleDto> Permissions { get; set; } = new List<PermissionRuleDto>();
}
