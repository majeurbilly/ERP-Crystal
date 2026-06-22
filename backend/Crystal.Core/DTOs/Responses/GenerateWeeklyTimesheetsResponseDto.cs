namespace Crystal.Core.DTOs.Responses;

public class GenerateWeeklyTimesheetsResponseDto
{
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public int? LocationId { get; set; }
    public int CreatedCount { get; set; }
    public int ExistingCount { get; set; }
    public int LinkedTimeEntryCount { get; set; }
    public IList<TimesheetResponseDto> Timesheets { get; set; } = new List<TimesheetResponseDto>();
}
