namespace Crystal.Core.Entities;

public class Receipt
{
    public int Id { get; set; }

    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;

    public int Quantity { get; set; }
    public DateOnly Date { get; set; }
}