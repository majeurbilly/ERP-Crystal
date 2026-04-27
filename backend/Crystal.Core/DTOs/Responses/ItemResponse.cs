namespace Crystal.Core.DTOs.Responses;

public class ItemResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public decimal Price { get; set; }
    public int AlertQuantity { get; set; }
    public int TotalQuantity { get; set; }

    public DateTime LastUpdate { get; set; }

    public bool HasBook { get; set; }
}
