namespace Crystal.Core.DTOs.Responses;

public class GeneratePayrollForPeriodResponseDto
{
    public int PayPeriodId { get; set; }
    public DateOnly PeriodStartDate { get; set; }
    public DateOnly PeriodEndDate { get; set; }
    public int? LocationId { get; set; }
    public int CreatedCount { get; set; }
    public int ExistingCount { get; set; }
    public int SkippedCount { get; set; }
    public IList<PayStubResponseDto> PayStubs { get; set; } = new List<PayStubResponseDto>();
}
