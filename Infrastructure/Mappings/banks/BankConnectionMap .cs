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
    public class BankConnectionMap : IEntityTypeConfiguration<BankConnection>
    {
        public void Configure(EntityTypeBuilder<BankConnection> builder)
        {
            builder.ToTable("bank_connection");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(x => x.ExternalConnectionId)
                .HasColumnName("external_connection_id")
                .HasMaxLength(200);

            builder.Property(x => x.InstitutionCode)
                .HasColumnName("institution_code")
                .HasMaxLength(100);

            builder.Property(x => x.InstitutionName)
                .HasColumnName("institution_name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Active)
                .HasColumnName("active")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.LastSyncAt)
                .HasColumnName("last_sync_at");

            builder.HasIndex(x => x.UserId);

            builder.HasIndex(x => x.Active);
        }
    }
}
