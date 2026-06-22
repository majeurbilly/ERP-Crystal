using Crystal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crystal.Infrastructure.Data.Configurations;

public class PayStubConfiguration : IEntityTypeConfiguration<PayStub>
{
    public void Configure(EntityTypeBuilder<PayStub> p_builder)
    {
        p_builder.Property(p_stub => p_stub.TotalHours)
            .HasPrecision(18, 2);

        p_builder.Property(p_stub => p_stub.GrossPay)
            .HasPrecision(18, 2);

        p_builder.Property(p_stub => p_stub.IsPublished)
            .IsRequired()
            .HasDefaultValue(false);

        p_builder.Property(p_stub => p_stub.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        p_builder.HasQueryFilter(p_stub => !p_stub.IsDeleted);

        p_builder.HasOne(p_stub => p_stub.PayPeriod)
            .WithMany(p_period => p_period.PayStubs)
            .HasForeignKey(p_stub => p_stub.PayPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        p_builder.HasOne(p_stub => p_stub.EmployeeProfile)
            .WithMany(p_profile => p_profile.PayStubs)
            .HasForeignKey(p_stub => p_stub.EmployeeProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        p_builder.HasOne(p_stub => p_stub.Timesheet)
            .WithMany()
            .HasForeignKey(p_stub => p_stub.TimesheetId)
            .OnDelete(DeleteBehavior.Restrict);

        p_builder.HasIndex(p_stub => new { p_stub.PayPeriodId, p_stub.EmployeeProfileId })
            .HasFilter("\"IsDeleted\" = false");
    }
}
