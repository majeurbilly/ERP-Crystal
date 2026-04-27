using System.ComponentModel.DataAnnotations;

namespace Crystal.Core.DTOs.Requests;

public class UpdateItemRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    public int AlertQuantity { get; set; }

    public int? BookId { get; set; }
}
