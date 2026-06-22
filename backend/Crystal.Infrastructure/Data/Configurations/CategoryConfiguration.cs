using Crystal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crystal.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> p_builder)
    {
        p_builder.Property(p_category => p_category.Name)
            .IsRequired()
            .HasMaxLength(100);

        p_builder.Property(p_category => p_category.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        p_builder.HasIndex(p_category => p_category.Name)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        p_builder.HasQueryFilter(p_category => !p_category.IsDeleted);
    }
}
