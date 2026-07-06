using Domain.Entities.Configuration;
using Domain.Entities.Goals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.Goals;

public sealed class CategoryBudgetGoalMap : IEntityTypeConfiguration<CategoryBudgetGoal>
{
    public void Configure(EntityTypeBuilder<CategoryBudgetGoal> builder)
    {
        builder.ToTable("category_budget_goals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(x => x.MonthlyLimit)
            .HasColumnName("monthly_limit")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .IsRequired();

        builder.Property(x => x.Active)
            .HasColumnName("active")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2(0)")
            .IsRequired();

        builder.Property(x => x.DeactivatedAt)
            .HasColumnName("deactivated_at")
            .HasColumnType("datetime2(0)");

        builder.HasIndex(x => new { x.UserId, x.CategoryId });

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
