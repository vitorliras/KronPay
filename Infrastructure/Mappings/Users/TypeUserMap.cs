using KronPay.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.Transactions
{
    public class TypeUserMap : IEntityTypeConfiguration<TypeUser>
    {
        public void Configure(EntityTypeBuilder<TypeUser> builder)
        {
            builder.ToTable("type_user");

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
