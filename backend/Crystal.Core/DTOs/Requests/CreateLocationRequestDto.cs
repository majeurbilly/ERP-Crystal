namespace Crystal.Core.DTOs.Requests;

public class CreateLocationRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
