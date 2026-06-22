using Crystal.Core.Entities;
using Crystal.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crystal.Infrastructure.Data.Configurations;

public class TimesheetConfiguration : IEntityTypeConfiguration<Timesheet>
{
    public void Configure(EntityTypeBuilder<Timesheet> p_builder)
    {
        p_builder.Property(p_timesheet => p_timesheet.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        p_builder.Property(p_timesheet => p_timesheet.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        p_builder.Property(p_timesheet => p_timesheet.IsPaid)
            .IsRequired()
            .HasDefaultValue(false);

        p_builder.HasQueryFilter(p_timesheet => !p_timesheet.IsDeleted);

        p_builder.HasOne(p_timesheet => p_timesheet.EmployeeProfile)
            .WithMany(p_profile => p_profile.Timesheets)
            .HasForeignKey(p_timesheet => p_timesheet.EmployeeProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        p_builder.HasIndex(p_timesheet => new { p_timesheet.EmployeeProfileId, p_timesheet.PeriodStart, p_timesheet.PeriodEnd })
            .HasFilter("\"IsDeleted\" = false");
    }
}
