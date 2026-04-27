namespace Crystal.Core.Entities;

public class Position
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MinimumSalary { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<WorkShift> WorkShifts { get; set; } = new List<WorkShift>();
}