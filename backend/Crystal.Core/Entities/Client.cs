namespace Crystal.Core.Entities;

public class Client
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
    public string RegionalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public DateOnly InscriptionDate { get; set; }
    public string Description { get; set; } = string.Empty;

    public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
}