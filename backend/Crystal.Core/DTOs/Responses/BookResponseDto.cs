namespace Crystal.Core.DTOs.Responses;

/// <summary>
/// Réponse détaillée d'un livre (endpoint /api/books et articles avec isBook).
/// </summary>
public class BookResponseDto : ItemResponseDto
{
    public string? Isbn { get; set; }

    public DateOnly? PublicationDate { get; set; }

    public List<string> Authors { get; set; } = [];

    public List<int> AuthorIds { get; set; } = [];

    public List<string> Publishers { get; set; } = [];

    public List<int> CategoryIds { get; set; } = [];

    public List<string> Categories { get; set; } = [];
}
