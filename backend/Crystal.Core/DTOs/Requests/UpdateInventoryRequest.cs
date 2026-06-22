namespace Crystal.Core.DTOs.Requests;

public class UpdateInventoryQuantityRequest
{
    public int ItemId { get; set; }
    public int LocationId { get; set; }
    public int Quantity { get; set; }
}