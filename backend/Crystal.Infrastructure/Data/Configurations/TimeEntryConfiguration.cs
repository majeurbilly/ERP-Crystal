using Crystal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crystal.Infrastructure.Data.Configurations;

public class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
{
    public void Configure(EntityTypeBuilder<TimeEntry> p_builder)
    {
        p_builder.Property(p_entry => p_entry.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        p_builder.HasQueryFilter(p_entry => !p_entry.IsDeleted);

        p_builder.HasOne(p_entry => p_entry.EmployeeProfile)
            .WithMany(p_profile => p_profile.TimeEntries)
            .HasForeignKey(p_entry => p_entry.EmployeeProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        p_builder.HasOne(p_entry => p_entry.ScheduledShift)
            .WithMany(p_shift => p_shift.TimeEntries)
            .HasForeignKey(p_entry => p_entry.ScheduledShiftId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        p_builder.HasOne(p_entry => p_entry.Timesheet)
            .WithMany(p_timesheet => p_timesheet.TimeEntries)
            .HasForeignKey(p_entry => p_entry.TimesheetId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
