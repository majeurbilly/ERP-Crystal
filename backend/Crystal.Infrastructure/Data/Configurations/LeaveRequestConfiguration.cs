using Crystal.Core.Entities;
using Crystal.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crystal.Infrastructure.Data.Configurations;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> p_builder)
    {
        p_builder.Property(p_request => p_request.LeaveType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        p_builder.Property(p_request => p_request.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        p_builder.Property(p_request => p_request.Reason)
            .HasMaxLength(500);

        p_builder.Property(p_request => p_request.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        p_builder.HasQueryFilter(p_request => !p_request.IsDeleted);

        p_builder.HasOne(p_request => p_request.EmployeeProfile)
            .WithMany(p_profile => p_profile.LeaveRequests)
            .HasForeignKey(p_request => p_request.EmployeeProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        p_builder.HasIndex(p_request => new { p_request.EmployeeProfileId, p_request.StartDate, p_request.EndDate })
            .HasFilter("\"IsDeleted\" = false");
    }
}
