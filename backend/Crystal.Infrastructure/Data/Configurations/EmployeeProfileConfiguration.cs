using Crystal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crystal.Infrastructure.Data.Configurations;

public class EmployeeProfileConfiguration : IEntityTypeConfiguration<EmployeeProfile>
{
    public void Configure(EntityTypeBuilder<EmployeeProfile> p_builder)
    {
        p_builder.Property(p_profile => p_profile.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        p_builder.Property(p_profile => p_profile.LastName)
            .IsRequired()
            .HasMaxLength(100);

        p_builder.Property(p_profile => p_profile.Email)
            .IsRequired()
            .HasMaxLength(256);

        p_builder.Property(p_profile => p_profile.Status)
            .IsRequired()
            .HasMaxLength(50);

        p_builder.Property(p_profile => p_profile.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        p_builder.HasIndex(p_profile => p_profile.Email)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        p_builder.HasIndex(p_profile => p_profile.ApplicationUserId)
            .IsUnique()
            .HasFilter("\"ApplicationUserId\" IS NOT NULL AND \"IsDeleted\" = false");

        p_builder.HasQueryFilter(p_profile => !p_profile.IsDeleted);

        p_builder.HasOne(p_profile => p_profile.ApplicationUser)
            .WithOne(p_user => p_user.EmployeeProfile)
            .HasForeignKey<EmployeeProfile>(p_profile => p_profile.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        p_builder.HasOne(p_profile => p_profile.JobPosition)
            .WithMany(p_position => p_position.EmployeeProfiles)
            .HasForeignKey(p_profile => p_profile.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        p_builder.HasOne(p_profile => p_profile.Location)
            .WithMany()
            .HasForeignKey(p_profile => p_profile.LocationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
