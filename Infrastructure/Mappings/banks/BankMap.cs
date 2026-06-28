using Domain.Entities.banks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.banks
{
    public class BankMap : IEntityTypeConfiguration<Bank>
    {
        public void Configure(EntityTypeBuilder<Bank> builder)
        {
            builder.ToTable("bank");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.PrimaryColor)
                .HasColumnName("primary_color")
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.InstitutionUrl)
                .HasColumnName("institution_url")
                .HasMaxLength(200);

            builder.Property(x => x.ImageUrl)
                .HasColumnName("image_url")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Type)
                .HasColumnName("type")
                .HasMaxLength(20);

            builder.Property(x => x.Active)
                .HasColumnName("active")
                .IsRequired();

        }
    }
}
