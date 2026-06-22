namespace Crystal.Core.DTOs.Responses;

public class LocationResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
