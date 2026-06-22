namespace Crystal.Core.Entities;

public class RolePermissionLocation
{
    public int RolePermissionId { get; set; }
    public RolePermission RolePermission { get; set; } = null!;
    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;
}
