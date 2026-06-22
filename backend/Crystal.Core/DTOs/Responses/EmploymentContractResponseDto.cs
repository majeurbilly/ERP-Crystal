namespace Crystal.Core.DTOs.Responses;

public class EmploymentContractResponseDto
{
    public int Id { get; set; }
    public int EmployeeProfileId { get; set; }
    public string EmployeeFirstName { get; set; } = string.Empty;
    public string EmployeeLastName { get; set; } = string.Empty;
    public string ContractType { get; set; } = string.Empty;
    public string WageType { get; set; } = string.Empty;
    public decimal BaseRate { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
