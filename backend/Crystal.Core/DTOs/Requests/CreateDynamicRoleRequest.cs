namespace Crystal.Core.DTOs.Requests;

public class CreateDynamicRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public IList<PermissionRuleRequest> Permissions { get; set; } = new List<PermissionRuleRequest>();
    public string? PresetId { get; set; }
}

public class PermissionRuleRequest
{
    public string Action { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? LocationScope { get; set; }
    public IList<int> LocationIds { get; set; } = new List<int>();
}
