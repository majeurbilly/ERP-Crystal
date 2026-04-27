namespace Crystal.Core.Entities;

public class Publisher
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<BookPublisher> BookPublishers { get; set; } = new List<BookPublisher>();
}