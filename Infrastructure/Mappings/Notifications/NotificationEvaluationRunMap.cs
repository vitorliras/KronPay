using Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.Notifications;

public sealed class NotificationEvaluationRunMap : IEntityTypeConfiguration<NotificationEvaluationRun>
{
    public void Configure(EntityTypeBuilder<NotificationEvaluationRun> builder)
    {
        builder.ToTable("notification_evaluation_runs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.LastRunAt)
            .HasColumnName("last_run_at")
            .HasColumnType("datetime2(0)")
            .IsRequired();
    }
}
