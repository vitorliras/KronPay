using Domain.Entities.Goals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.Goals;

public sealed class FinancialGoalMap : IEntityTypeConfiguration<FinancialGoal>
{
    public void Configure(EntityTypeBuilder<FinancialGoal> builder)
    {
        builder.ToTable("financial_goals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.TargetAmount)
            .HasColumnName("target_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.CurrentAmount)
            .HasColumnName("current_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.TargetDate)
            .HasColumnName("target_date")
            .HasColumnType("datetime2(0)")
            .IsRequired();

        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2(0)")
            .IsRequired();

        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at")
            .HasColumnType("datetime2(0)");

        builder.Property(x => x.PreviousAttemptGoalId)
            .HasColumnName("previous_attempt_goal_id");

        builder.HasIndex(x => new { x.UserId, x.Status });
    }
}
