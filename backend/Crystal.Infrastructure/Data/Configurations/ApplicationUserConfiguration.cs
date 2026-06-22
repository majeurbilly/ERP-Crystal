using Crystal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crystal.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> p_builder)
    {
        p_builder.HasOne(p_user => p_user.DynamicRole)
            .WithMany(p_role => p_role.Users)
            .HasForeignKey(p_user => p_user.DynamicRoleId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
