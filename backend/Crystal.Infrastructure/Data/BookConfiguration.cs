using Crystal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crystal.Infrastructure.Data.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> p_builder)
    {
        p_builder.HasKey(p_b => p_b.ItemId);

        p_builder.HasOne(p_b => p_b.Item)
            .WithOne(p_i => p_i.Book)
            .HasForeignKey<Book>(p_b => p_b.ItemId);

        p_builder.Property(p_b => p_b.PublicationDate)
            .IsRequired();

        p_builder.Property(p_b => p_b.Isbn)
            .HasMaxLength(20);
    }
}
