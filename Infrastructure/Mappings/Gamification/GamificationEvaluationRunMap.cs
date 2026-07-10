using Domain.Entities.Gamification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.Gamification;

public sealed class GamificationEvaluationRunMap : IEntityTypeConfiguration<GamificationEvaluationRun>
{
    public void Configure(EntityTypeBuilder<GamificationEvaluationRun> builder)
    {
        builder.ToTable("gamification_evaluation_runs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.RanAt)
            .HasColumnName("ran_at")
            .HasColumnType("datetime2(0)")
            .IsRequired();

        builder.Property(x => x.UsersProcessed)
            .HasColumnName("users_processed")
            .IsRequired();

        builder.Property(x => x.EventsTriggered)
            .HasColumnName("events_triggered")
            .IsRequired();

        builder.Property(x => x.BadgesUnlocked)
            .HasColumnName("badges_unlocked")
            .IsRequired();
    }
}
