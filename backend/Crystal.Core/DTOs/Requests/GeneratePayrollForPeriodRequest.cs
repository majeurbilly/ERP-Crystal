namespace Crystal.Core.DTOs.Requests;

public class GeneratePayrollForPeriodRequest
{
    public int PayPeriodId { get; set; }
    public int? LocationId { get; set; }
}
