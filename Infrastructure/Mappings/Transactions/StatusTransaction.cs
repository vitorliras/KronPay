using Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.Transactions
{
    public class StatusTransactionMap : IEntityTypeConfiguration<StatusTransaction>
    {
        public void Configure(EntityTypeBuilder<StatusTransaction> builder)
        {
            builder.ToTable("status_transaction");

            builder.HasKey(x => x.Code);

            builder.Property(x => x.Code)
                .HasColumnName("code")
                .HasColumnType("char(1)")
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(20)
                .IsRequired();
        }
    }
}
