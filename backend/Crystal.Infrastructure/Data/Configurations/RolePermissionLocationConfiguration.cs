using Crystal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crystal.Infrastructure.Data.Configurations;

public class RolePermissionLocationConfiguration : IEntityTypeConfiguration<RolePermissionLocation>
{
    public void Configure(EntityTypeBuilder<RolePermissionLocation> p_builder)
    {
        p_builder.HasKey(p_link => new { p_link.RolePermissionId, p_link.LocationId });

        p_builder.HasOne(p_link => p_link.RolePermission)
            .WithMany(p_permission => p_permission.ScopedLocations)
            .HasForeignKey(p_link => p_link.RolePermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        p_builder.HasOne(p_link => p_link.Location)
            .WithMany()
            .HasForeignKey(p_link => p_link.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
