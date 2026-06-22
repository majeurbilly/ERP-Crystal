using Crystal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crystal.Infrastructure.Data.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> p_builder)
    {
        p_builder.Property(p_location => p_location.Title)
            .IsRequired()
            .HasMaxLength(100);

        p_builder.Property(p_location => p_location.Address)
            .IsRequired()
            .HasMaxLength(200);

        p_builder.Property(p_location => p_location.Description)
            .IsRequired()
            .HasMaxLength(500);

        p_builder.HasIndex(p_location => p_location.Title)
            .IsUnique();
    }
}
