using Domain.Entities.AI;
using KronPay.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.AI;

public sealed class AiUsageLogMap : IEntityTypeConfiguration<AiUsageLog>
{
    public void Configure(EntityTypeBuilder<AiUsageLog> builder)
    {
        builder.ToTable("ai_usage_log");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Feature)
            .HasColumnName("feature")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Provider)
            .HasColumnName("provider")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Model)
            .HasColumnName("model")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.InputTokens)
            .HasColumnName("input_tokens")
            .IsRequired();

        builder.Property(x => x.CachedInputTokens)
            .HasColumnName("cached_input_tokens")
            .IsRequired();

        builder.Property(x => x.OutputTokens)
            .HasColumnName("output_tokens")
            .IsRequired();

        builder.Property(x => x.EstimatedCostUsd)
            .HasColumnName("estimated_cost_usd")
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasPrecision(0)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.CreatedAt });
    }
}
