namespace Crystal.Core.Entities;

public class WorkShift
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int PositionId { get; set; }
    public Position Position { get; set; } = null!;

    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public ICollection<Punch> Punches { get; set; } = new List<Punch>();
}