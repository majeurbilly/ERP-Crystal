namespace Crystal.Core.Entities;

public class JobPosition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#3B82F6";

    public bool IsDeleted { get; set; } = false;

    public ICollection<EmployeeProfile> EmployeeProfiles { get; set; } = new List<EmployeeProfile>();
    public ICollection<ScheduledShift> ScheduledShifts { get; set; } = new List<ScheduledShift>();
}
