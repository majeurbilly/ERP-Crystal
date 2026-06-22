namespace Crystal.Core.DTOs.Requests;

public class GenerateWeeklyTimesheetsRequest
{
    public DateOnly PeriodStart { get; set; }
    public int? LocationId { get; set; }
}
