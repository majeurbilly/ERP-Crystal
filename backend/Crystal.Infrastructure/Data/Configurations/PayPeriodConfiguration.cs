using Crystal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crystal.Infrastructure.Data.Configurations;

public class PayPeriodConfiguration : IEntityTypeConfiguration<PayPeriod>
{
    public void Configure(EntityTypeBuilder<PayPeriod> p_builder)
    {
        p_builder.Property(p_period => p_period.IsProcessed)
            .IsRequired()
            .HasDefaultValue(false);

        p_builder.HasIndex(p_period => new { p_period.StartDate, p_period.EndDate });
    }
}
