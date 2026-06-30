using Domain.Entities.Card;
using Domain.Entities.Configuration;
using KronPay.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.Card;

public sealed class CardPurchaseMap : IEntityTypeConfiguration<CardPurchase>
{
    public void Configure(EntityTypeBuilder<CardPurchase> builder)
    {
        builder.ToTable("card_purchase");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.CreditCardId)
            .HasColumnName("credit_card_id")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.TotalAmount)
            .HasColumnName("total_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.PurchaseDate)
            .HasColumnName("purchase_date")
            .HasColumnType("datetime2(0)")
            .IsRequired();

        builder.Property(x => x.InstallmentsCount)
            .HasColumnName("installments_count")
            .IsRequired();

        builder.Property(x => x.CategoryId)
            .HasColumnName("category_id");

        builder.Property(x => x.CategoryItemId)
            .HasColumnName("category_item_id");

        builder.Property(x => x.Origin)
            .HasColumnName("origin")
            .HasColumnType("char(1)")
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

        builder.HasOne<CreditCard>()
            .WithMany()
            .HasForeignKey(x => x.CreditCardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .HasPrincipalKey(x => x.Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CategoryItem>()
            .WithMany()
            .HasForeignKey(x => x.CategoryItemId)
            .HasPrincipalKey(x => x.Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.CreditCardId });
    }
}
