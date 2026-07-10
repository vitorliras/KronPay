using Domain.Entities.Gamification;
using KronPay.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.Gamification;

public sealed class ConsistencyCounterMap : IEntityTypeConfiguration<ConsistencyCounter>
{
    public void Configure(EntityTypeBuilder<ConsistencyCounter> builder)
    {
        builder.ToTable("consistency_counters");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.CounterKey)
            .HasColumnName("counter_key")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.CurrentStreak)
            .HasColumnName("current_streak")
            .IsRequired();

        builder.Property(x => x.BestStreak)
            .HasColumnName("best_streak")
            .IsRequired();

        builder.Property(x => x.LastUpdatedAt)
            .HasColumnName("last_updated_at")
            .HasColumnType("datetime2(0)")
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.CounterKey }).IsUnique();
    }
}
