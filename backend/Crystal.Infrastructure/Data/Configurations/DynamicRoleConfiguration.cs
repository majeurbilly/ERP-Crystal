using Crystal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crystal.Infrastructure.Data.Configurations;

public class DynamicRoleConfiguration : IEntityTypeConfiguration<DynamicRole>
{
    public void Configure(EntityTypeBuilder<DynamicRole> p_builder)
    {
        p_builder.HasKey(p_role => p_role.Id);

        p_builder.Property(p_role => p_role.Id)
            .HasMaxLength(64);

        p_builder.Property(p_role => p_role.Name)
            .IsRequired()
            .HasMaxLength(128);

        p_builder.Property(p_role => p_role.IsPreset)
            .IsRequired()
            .HasDefaultValue(false);

        p_builder.HasMany(p_role => p_role.Permissions)
            .WithOne(p_permission => p_permission.DynamicRole)
            .HasForeignKey(p_permission => p_permission.DynamicRoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
