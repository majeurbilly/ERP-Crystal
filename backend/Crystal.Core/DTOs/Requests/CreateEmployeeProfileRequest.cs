namespace Crystal.Core.DTOs.Requests;

public class CreateEmployeeProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ApplicationUserId { get; set; }
    public decimal Salary { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? JobPositionId { get; set; }
    public DateOnly HiringDate { get; set; }
    public int? LocationId { get; set; }
}
