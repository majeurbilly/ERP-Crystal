namespace Crystal.Core.Entities;

public class Author
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<AuthorBook> AuthorBooks { get; set; } = new List<AuthorBook>();
}