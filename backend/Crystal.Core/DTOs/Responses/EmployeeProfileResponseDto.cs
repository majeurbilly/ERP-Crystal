namespace Crystal.Core.DTOs.Responses;

public class EmployeeProfileResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ApplicationUserId { get; set; }
    public DateOnly HiringDate { get; set; }
    public decimal Salary { get; set; }
    public string Status { get; set; } = string.Empty;
    public int JobPositionId { get; set; }
    public string JobPositionName { get; set; } = string.Empty;
    public int? LocationId { get; set; }
    public string? LocationTitle { get; set; }
}
