using Crystal.Core.Enums;

namespace Crystal.Core.DTOs.Requests;

public class UpdateEmploymentContractRequest
{
    public int EmployeeProfileId { get; set; }
    public ContractType ContractType { get; set; }
    public WageType WageType { get; set; }
    public decimal BaseRate { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
