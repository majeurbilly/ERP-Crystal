using Crystal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crystal.Infrastructure.Data.Configurations;

public class JobPositionConfiguration : IEntityTypeConfiguration<JobPosition>
{
    public void Configure(EntityTypeBuilder<JobPosition> p_builder)
    {
        p_builder.Property(p_position => p_position.Name)
            .IsRequired()
            .HasMaxLength(100);

        p_builder.Property(p_position => p_position.Description)
            .IsRequired()
            .HasMaxLength(500);

        p_builder.Property(p_position => p_position.Color)
            .IsRequired()
            .HasMaxLength(7)
            .HasDefaultValue("#3B82F6");

        p_builder.Property(p_position => p_position.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        p_builder.HasIndex(p_position => p_position.Name)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        p_builder.HasQueryFilter(p_position => !p_position.IsDeleted);
    }
}
