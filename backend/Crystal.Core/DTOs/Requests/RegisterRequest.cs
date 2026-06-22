using Crystal.Core;
using System.ComponentModel.DataAnnotations;

namespace Crystal.Core.DTOs.Requests;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string DynamicRoleId { get; set; } = ApplicationRoles.Employee;
}
