using Crystal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crystal.Infrastructure.Data.Configurations;

public class ScheduledShiftConfiguration : IEntityTypeConfiguration<ScheduledShift>
{
    public void Configure(EntityTypeBuilder<ScheduledShift> p_builder)
    {
        p_builder.Property(p_shift => p_shift.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        p_builder.HasQueryFilter(p_shift => !p_shift.IsDeleted);

        p_builder.HasOne(p_shift => p_shift.EmployeeProfile)
            .WithMany(p_profile => p_profile.ScheduledShifts)
            .HasForeignKey(p_shift => p_shift.EmployeeProfileId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        p_builder.HasOne(p_shift => p_shift.JobPosition)
            .WithMany(p_position => p_position.ScheduledShifts)
            .HasForeignKey(p_shift => p_shift.JobPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        p_builder.HasOne(p_shift => p_shift.Location)
            .WithMany()
            .HasForeignKey(p_shift => p_shift.LocationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
