namespace Crystal.Core.DTOs.Requests;

public class UpdateBookRequest
{
    public string? Isbn { get; set; }

    public DateOnly? PublicationDate { get; set; }

    public List<int> AuthorIds { get; set; } = new();
    public List<int> CategoryIds { get; set; } = new();
    public List<int> PublisherIds { get; set; } = new();
}
