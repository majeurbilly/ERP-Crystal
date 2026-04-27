namespace Crystal.Core.Entities;

public class InventoryLine
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;
    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;

    public int Quantity { get; set; }
}