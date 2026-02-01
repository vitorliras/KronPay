using KronPay.Domain.Entities;
using KronPay.Domain.Entities.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KronPay.Infra.Data.Mappings.Configuration
{
    public class TypePaymentMethodMap : IEntityTypeConfiguration<TypePaymentMethod>
    {
        public void Configure(EntityTypeBuilder<TypePaymentMethod> builder)
        {
            builder.ToTable("type_payment_method");

            builder.HasKey(x => x.Code);

            builder.Property(x => x.Code)
                .HasColumnName("code")
                .HasColumnType("char(1)")
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}
