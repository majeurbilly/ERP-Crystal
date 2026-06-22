namespace Crystal.Core.DTOs.Requests;

public class UpdateItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Distributor { get; set; }
    public decimal Price { get; set; }
    public int AlertQuantity { get; set; }
    public bool? IsActive { get; set; }

    public List<int>? CategoryIds { get; set; }

    public string? Isbn { get; set; }

    public DateOnly? PublicationDate { get; set; }

    public List<string>? Authors { get; set; }

    public List<string>? Publishers { get; set; }
}