namespace Crystal.Core.DTOs.Requests;

public class CreatePayPeriodRequest
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
