using Domain.Entities;
using Domain.Entities.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings;

public sealed class CategoryItemMap : IEntityTypeConfiguration<CategoryItem>
{
    public void Configure(EntityTypeBuilder<CategoryItem> builder)
    {
        builder.ToTable("category_item");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.Active)
            .HasColumnName("active")
            .IsRequired();

        builder.Property(x => x.DeactivatedAt)
            .HasColumnName("deactivated_at");


        builder
            .HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId);
    }
}
