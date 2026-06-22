namespace Crystal.Core.DTOs.Responses;

public class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string DynamicRoleId { get; set; } = string.Empty;
    public string? DynamicRoleName { get; set; }
}
