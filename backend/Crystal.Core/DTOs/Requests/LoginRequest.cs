using System.ComponentModel.DataAnnotations;

namespace Crystal.Core.DTOs.Requests;

public class LoginRequest : IValidatableObject
{
    public string? Username { get; set; }

    public string? Email { get; set; }

    [Required]
    public string Password { get; set; } = string.Empty;

    public string GetLoginIdentifier() =>
        !string.IsNullOrWhiteSpace(Email) ? Email.Trim() : Username?.Trim() ?? string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext p_validationContext)
    {
        if (string.IsNullOrWhiteSpace(GetLoginIdentifier()))
        {
            yield return new ValidationResult(
                "Username or email is required.",
                [nameof(Username), nameof(Email)]);
        }
    }
}
