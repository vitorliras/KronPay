using Domain.Entities.Card;
using KronPay.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.Card;

public sealed class CardInstallmentMap : IEntityTypeConfiguration<CardInstallment>
{
    public void Configure(EntityTypeBuilder<CardInstallment> builder)
    {
        builder.ToTable("card_installment");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.CardPurchaseId)
            .HasColumnName("card_purchase_id")
            .IsRequired();

        builder.Property(x => x.CardInvoiceId)
            .HasColumnName("card_invoice_id")
            .IsRequired();

        builder.Property(x => x.InstallmentNumber)
            .HasColumnName("installment_number")
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("char(1)")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CardPurchase)
            .WithMany()
            .HasForeignKey(x => x.CardPurchaseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CardInvoice)
            .WithMany()
            .HasForeignKey(x => x.CardInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CardInvoiceId);
        builder.HasIndex(x => x.CardPurchaseId);
    }
}
