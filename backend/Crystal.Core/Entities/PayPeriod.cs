namespace Crystal.Core.Entities;

public class PayPeriod
{
    public int Id { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsProcessed { get; set; } = false;

    public ICollection<PayStub> PayStubs { get; set; } = new List<PayStub>();
}
