namespace Crystal.Core.DTOs.Responses;

public class PermissionRuleDto
{
    public string Action { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? LocationScope { get; set; }
    public IList<int> LocationIds { get; set; } = new List<int>();
}
