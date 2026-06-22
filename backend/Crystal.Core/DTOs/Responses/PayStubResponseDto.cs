namespace Crystal.Core.DTOs.Responses;

public class PayStubResponseDto
{
    public int Id { get; set; }
    public int PayPeriodId { get; set; }
    public int EmployeeProfileId { get; set; }
    public string EmployeeFirstName { get; set; } = string.Empty;
    public string EmployeeLastName { get; set; } = string.Empty;
    public DateOnly PeriodStartDate { get; set; }
    public DateOnly PeriodEndDate { get; set; }
    public decimal TotalHours { get; set; }
    public decimal GrossPay { get; set; }
    public bool IsPublished { get; set; }
}
