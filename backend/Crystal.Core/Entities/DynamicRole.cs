namespace Crystal.Core.Entities;

public class DynamicRole
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsPreset { get; set; }

    public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}
