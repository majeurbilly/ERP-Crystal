namespace Crystal.Core.Entities;

public class RolePermission
{
    public int Id { get; set; }
    public string DynamicRoleId { get; set; } = string.Empty;
    public DynamicRole DynamicRole { get; set; } = null!;
    public string Action { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? LocationScope { get; set; }

    public ICollection<RolePermissionLocation> ScopedLocations { get; set; } = new List<RolePermissionLocation>();
}
