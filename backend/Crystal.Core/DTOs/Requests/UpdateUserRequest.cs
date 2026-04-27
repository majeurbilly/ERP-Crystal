using System.ComponentModel.DataAnnotations;

namespace Crystal.Core.DTOs.Requests;

public class UpdateUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}
