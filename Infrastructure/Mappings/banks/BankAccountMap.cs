using Domain.Entities.banks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Mappings.banks
{
    public class BankAccountMap : IEntityTypeConfiguration<BankAccount>
    {
        public void Configure(EntityTypeBuilder<BankAccount> builder)
        {
            builder.ToTable("bank_account");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.BankConnectionId)
                .HasColumnName("bank_connection_id")
                .IsRequired();

            builder.Property(x => x.ExternalAccountId)
                .HasColumnName("external_account_id")
                .HasMaxLength(200);

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Type)
                .HasColumnName("type")
                .HasMaxLength(100);

            builder.Property(x => x.CurrentBalance)
                .HasColumnName("current_balance")
                .HasPrecision(18, 2);

            builder.Property(x => x.Active)
                .HasColumnName("active")
                .IsRequired();

            builder.HasIndex(x => x.BankConnectionId);

            builder.HasOne(x => x.BankConnection)
                .WithMany(x => x.BankAccounts)
                .HasForeignKey(x => x.BankConnectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
