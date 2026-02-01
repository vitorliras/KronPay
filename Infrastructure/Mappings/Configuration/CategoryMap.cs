using Domain.Entities;
using Domain.Entities.Configuration;
using KronPay.Domain.Entities.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings;

public sealed class CategoryMap : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("category");
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.CodTypeTransaction)
            .HasColumnName("cod_type_transaction")
            .HasColumnType("char(1)")
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
            .HasColumnType("bit")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.DeactivatedAt)
            .HasColumnName("deactivated_at");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TypeTransaction>()
            .WithMany()
            .HasForeignKey(x => x.CodTypeTransaction)
            .HasPrincipalKey(x => x.Code)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.Description })
            .IsUnique();
    }
}
