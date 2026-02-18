using Domain.Entities.Configuration;
using Domain.Entities.Transactions;
using KronPay.Domain.Entities.Configuration;
using KronPay.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.Transactions;

public sealed class TransactionMap : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transaction");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.TransactionDate)
            .HasColumnName("transaction_date")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.CodTypeTransaction)
            .HasColumnName("cod_type_transaction")
            .HasColumnType("char(1)")
            .IsRequired();

        builder.Property(x => x.IdPaymentMethod)
            .HasColumnName("id_payment_method")
            .HasColumnType("int")
            .IsRequired();

        builder.HasOne<TypeTransaction>()
            .WithMany()
            .HasForeignKey(x => x.CodTypeTransaction)
            .HasPrincipalKey(x => x.Code)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("char(1)")
            .IsRequired();

        builder.Property(x => x.CategoryId)
            .HasColumnName("category_id");

        builder.Property(x => x.CategoryItemId)
            .HasColumnName("category_item_id");

        builder.Property(x => x.TransactionGroupId)
            .HasColumnName("transaction_group_id");

        builder.Property(x => x.CodTypeTransaction)
            .HasColumnName("cod_type_transaction");

        builder.HasOne(x => x.TransactionGroup)
             .WithMany()
             .HasForeignKey(x => x.TransactionGroupId)
             .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<StatusTransaction>()
            .WithMany()
            .HasForeignKey(x => x.Status)
            .HasPrincipalKey(x => x.Code)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TypeTransaction>()
           .WithMany()
           .HasForeignKey(x => x.CodTypeTransaction)
           .HasPrincipalKey(x => x.Code)
           .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PaymentMethod>()
            .WithMany()
            .HasForeignKey(x => x.IdPaymentMethod)
            .HasPrincipalKey(x => x.Id)
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

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.TransactionDate);
        builder.HasIndex(x => x.TransactionGroupId);
    }
}
