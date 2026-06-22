using Crystal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crystal.Infrastructure.Data.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> p_builder)
    {
        p_builder.HasIndex(p_permission => new
        {
            p_permission.DynamicRoleId,
            p_permission.Action,
            p_permission.Subject,
        })
            .IsUnique();

        p_builder.Property(p_permission => p_permission.Action)
            .IsRequired()
            .HasMaxLength(32);

        p_builder.Property(p_permission => p_permission.Subject)
            .IsRequired()
            .HasMaxLength(64);

        p_builder.Property(p_permission => p_permission.LocationScope)
            .HasMaxLength(16);
    }
}
