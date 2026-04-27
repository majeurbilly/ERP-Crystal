namespace Crystal.Core.Entities;

public class Employee
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public decimal Salary { get; set; }
    public string Status { get; set; } = string.Empty;

    public int RoleId { get; set; }
    public EmployeeRole Role { get; set; } = null!;

    public int PositionId { get; set; }
    public Position Position { get; set; } = null!;

    public DateOnly HiringDate { get; set; }

    public ICollection<WorkShift> WorkShifts { get; set; } = new List<WorkShift>();
    public ICollection<Punch> Punches { get; set; } = new List<Punch>();
    public ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
}