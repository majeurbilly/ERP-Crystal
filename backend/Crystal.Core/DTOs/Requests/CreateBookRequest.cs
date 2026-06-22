namespace Crystal.Core.DTOs.Requests;

public class CreateBookRequest : CreateItemRequest
{
    public string? Isbn { get; set; }

    public DateOnly PublicationDate { get; set; }

    public List<string> Authors { get; set; } = new();
    public List<string> Publishers { get; set; } = new();

    public List<int> AuthorIds { get; set; } = new();
    public List<int> CategoryIds { get; set; } = new();
    public List<int> PublisherIds { get; set; } = new();
}