namespace Crystal.Core.DTOs.Responses;

public class RegisterResult
{
    public bool Succeeded { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}
