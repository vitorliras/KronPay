using Domain.Entities.Configuration;
using Domain.Entities.Planning;
using KronPay.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.Planning;

public sealed class PlannedCommitmentMap : IEntityTypeConfiguration<PlannedCommitment>
{
    public void Configure(EntityTypeBuilder<PlannedCommitment> builder)
    {
        builder.ToTable("planned_commitment");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Direction)
            .HasColumnName("direction")
            .HasColumnType("char(1)")
            .IsRequired();

        builder.Property(x => x.Periodicity)
            .HasColumnName("periodicity")
            .HasColumnType("char(1)")
            .IsRequired();

        builder.Property(x => x.StartDate)
            .HasColumnName("start_date")
            .HasColumnType("datetime2(0)")
            .IsRequired();

        builder.Property(x => x.EndDate)
            .HasColumnName("end_date")
            .HasColumnType("datetime2(0)");

        builder.Property(x => x.CategoryId)
            .HasColumnName("category_id");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.Active)
            .HasColumnName("active")
            .HasColumnType("bit")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.DeactivatedAt)
            .HasColumnName("deactivated_at");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .HasPrincipalKey(x => x.Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.Active });
    }
}
