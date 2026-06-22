namespace Crystal.Core.DTOs.Responses;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string DynamicRoleId { get; set; } = string.Empty;
}
