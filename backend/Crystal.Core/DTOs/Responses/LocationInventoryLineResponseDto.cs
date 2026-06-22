namespace Crystal.Core.DTOs.Responses;

public class LocationInventoryLineResponseDto
{
    public int LocationId { get; set; }
    public string LocationTitle { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
