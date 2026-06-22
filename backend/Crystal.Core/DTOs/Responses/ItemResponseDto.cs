using System.Collections.Generic;

namespace Crystal.Core.DTOs.Responses;

public class ItemResponseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Distributor { get; set; }
    public string? ImageUrl { get; set; }

    public decimal Price { get; set; }

    public int TotalQuantity { get; set; }

    public int AlertQuantity { get; set; }

    public bool IsLowStock { get; set; }

    public DateTime LastUpdate { get; set; }

    public bool IsBook { get; set; }

    public bool IsActive { get; set; }
}