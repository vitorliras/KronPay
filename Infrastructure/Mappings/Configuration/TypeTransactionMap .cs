using KronPay.Domain.Entities;
using KronPay.Domain.Entities.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KronPay.Infra.Data.Mappings.Configuration
{
    public class TypeTransactionMap : IEntityTypeConfiguration<TypeTransaction>
    {
        public void Configure(EntityTypeBuilder<TypeTransaction> builder)
        {
            builder.ToTable("type_transaction");

            builder.HasKey(x => x.Codigo);

            builder.Property(x => x.Codigo)
                .HasColumnName("codigo")
                .HasColumnType("char(1)")
                .IsRequired();

            builder.Property(x => x.Descricao)
                .HasColumnName("descricao")
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}
