namespace Crystal.Core.DTOs.Requests;

public class UpdateDynamicRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public IList<PermissionRuleRequest> Permissions { get; set; } = new List<PermissionRuleRequest>();
}
