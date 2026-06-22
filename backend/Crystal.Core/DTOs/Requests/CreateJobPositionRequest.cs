namespace Crystal.Core.DTOs.Requests;

public class CreateJobPositionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#3B82F6";
}
