using Crystal.Core.Entities;
using Crystal.Infrastructure.Data.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crystal.Infrastructure.Data.Configurations;

public class EmploymentContractConfiguration : IEntityTypeConfiguration<EmploymentContract>
{
    public void Configure(EntityTypeBuilder<EmploymentContract> p_builder)
    {
        p_builder.Property(p_contract => p_contract.ContractType)
            .IsRequired()
            .HasConversion(LegacyEnumConverters.ForContractType)
            .HasMaxLength(20);

        p_builder.Property(p_contract => p_contract.WageType)
            .IsRequired()
            .HasConversion(LegacyEnumConverters.ForWageType)
            .HasMaxLength(20);

        p_builder.Property(p_contract => p_contract.BaseRate)
            .HasPrecision(18, 2);

        p_builder.Property(p_contract => p_contract.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        p_builder.HasQueryFilter(p_contract => !p_contract.IsDeleted);

        p_builder.HasOne(p_contract => p_contract.EmployeeProfile)
            .WithMany(p_profile => p_profile.EmploymentContracts)
            .HasForeignKey(p_contract => p_contract.EmployeeProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        p_builder.HasIndex(p_contract => new { p_contract.EmployeeProfileId, p_contract.StartDate })
            .HasFilter("\"IsDeleted\" = false");
    }
}
