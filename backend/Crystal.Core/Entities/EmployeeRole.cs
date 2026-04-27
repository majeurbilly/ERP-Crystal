namespace Crystal.Core.Entities;

public class EmployeeRole
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}