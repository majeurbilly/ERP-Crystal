namespace Crystal.Core.Entities;

public class Book
{
    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;

    public string? Isbn { get; set; }

    public DateOnly PublicationDate { get; set; }

    public ICollection<AuthorBook> AuthorBooks { get; set; } = new List<AuthorBook>();
    public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
    public ICollection<BookPublisher> BookPublishers { get; set; } = new List<BookPublisher>();
}