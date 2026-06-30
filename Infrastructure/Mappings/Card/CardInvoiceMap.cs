using Domain.Entities.Card;
using Domain.Entities.Transactions;
using KronPay.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.Card;

public sealed class CardInvoiceMap : IEntityTypeConfiguration<CardInvoice>
{
    public void Configure(EntityTypeBuilder<CardInvoice> builder)
    {
        builder.ToTable("card_invoice");

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

        builder.Property(x => x.ReferenceYear)
            .HasColumnName("reference_year")
            .IsRequired();

        builder.Property(x => x.ReferenceMonth)
            .HasColumnName("reference_month")
            .IsRequired();

        builder.Property(x => x.ClosingDate)
            .HasColumnName("closing_date")
            .HasColumnType("datetime2(0)")
            .IsRequired();

        builder.Property(x => x.DueDate)
            .HasColumnName("due_date")
            .HasColumnType("datetime2(0)")
            .IsRequired();

        builder.Property(x => x.TotalAmount)
            .HasColumnName("total_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("char(1)")
            .IsRequired();

        builder.Property(x => x.PaidAt)
            .HasColumnName("paid_at");

        builder.Property(x => x.TransactionId)
            .HasColumnName("transaction_id");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CreditCard>()
            .WithMany()
            .HasForeignKey(x => x.CreditCardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Transaction)
            .WithMany()
            .HasForeignKey(x => x.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.CreditCardId, x.ReferenceYear, x.ReferenceMonth })
            .IsUnique();
    }
}
