using Crystal.Core.Enums;

namespace Crystal.Core.Entities;

public class EmploymentContract
{
    public int Id { get; set; }

    public int EmployeeProfileId { get; set; }
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    public ContractType ContractType { get; set; }
    public WageType WageType { get; set; }
    public decimal BaseRate { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public bool IsDeleted { get; set; } = false;
}
