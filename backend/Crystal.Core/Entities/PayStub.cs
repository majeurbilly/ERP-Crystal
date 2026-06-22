namespace Crystal.Core.Entities;

public class PayStub
{
    public int Id { get; set; }

    public int PayPeriodId { get; set; }
    public PayPeriod PayPeriod { get; set; } = null!;

    public int EmployeeProfileId { get; set; }
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    public int? TimesheetId { get; set; }
    public Timesheet? Timesheet { get; set; }

    public decimal TotalHours { get; set; }
    public decimal GrossPay { get; set; }
    public bool IsPublished { get; set; } = false;

    public bool IsDeleted { get; set; } = false;
}
