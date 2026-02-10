using Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.Transactions;

public sealed class TransactionGroupMap : IEntityTypeConfiguration<TransactionGroup>
{
    public void Configure(EntityTypeBuilder<TransactionGroup> builder)
    {
        builder.ToTable("transaction_group");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasColumnType("char(1)")
            .IsRequired();

        builder.Property(x => x.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(x => x.EndDate)
            .HasColumnName("end_date");

        builder.Property(x => x.Installments)
            .HasColumnName("installments");

        builder.Property(x => x.Active)
            .HasColumnName("active")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Active);
    }
}
