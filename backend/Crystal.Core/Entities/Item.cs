namespace Crystal.Core.Entities;

public class Item
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Distributor { get; set; }
    public string? ImageUrl { get; set; }

    public decimal Price { get; set; }
    public int AlertQuantity { get; set; }
    public DateTime LastUpdate { get; set; }

    public Book? Book { get; set; }
    public bool IsActive { get; set; }
    public ICollection<InventoryLine> InventoryLines { get; set; } = new List<InventoryLine>();
    public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
}