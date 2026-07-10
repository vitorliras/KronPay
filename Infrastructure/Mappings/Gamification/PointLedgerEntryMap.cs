using Domain.Entities.Gamification;
using KronPay.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.Gamification;

public sealed class PointLedgerEntryMap : IEntityTypeConfiguration<PointLedgerEntry>
{
    public void Configure(EntityTypeBuilder<PointLedgerEntry> builder)
    {
        builder.ToTable("point_ledger_entries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(x => x.Significance)
            .HasColumnName("significance")
            .IsRequired();

        builder.Property(x => x.PointsDelta)
            .HasColumnName("points_delta")
            .IsRequired();

        builder.Property(x => x.TierAtEvent)
            .HasColumnName("tier_at_event")
            .IsRequired();

        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .HasColumnType("datetime2(0)")
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.OccurredAt });
    }
}
