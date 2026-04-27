using System.ComponentModel.DataAnnotations;

namespace Crystal.Core.DTOs.Requests;

public class UpdateProfileRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string UserName { get; set; } = string.Empty;
}
