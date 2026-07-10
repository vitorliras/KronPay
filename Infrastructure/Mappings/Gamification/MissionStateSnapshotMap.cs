using Domain.Entities.Gamification;
using KronPay.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.Gamification;

public sealed class MissionStateSnapshotMap : IEntityTypeConfiguration<MissionStateSnapshot>
{
    public void Configure(EntityTypeBuilder<MissionStateSnapshot> builder)
    {
        builder.ToTable("mission_state_snapshots");

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

        builder.Property(x => x.RelatedEntityId)
            .HasColumnName("related_entity_id");

        builder.Property(x => x.IsConditionActive)
            .HasColumnName("is_condition_active")
            .HasColumnType("bit")
            .IsRequired();

        builder.Property(x => x.LastEvaluatedAt)
            .HasColumnName("last_evaluated_at")
            .HasColumnType("datetime2(0)")
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.Type, x.RelatedEntityId }).IsUnique();
    }
}
