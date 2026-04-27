namespace Crystal.Core.Entities;

public class BookPublisher
{
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    public int PublisherId { get; set; }
    public Publisher Publisher { get; set; } = null!;
}