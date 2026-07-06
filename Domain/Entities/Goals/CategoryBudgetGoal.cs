using Domain.Enums.Goals;
using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.Goals;

public sealed class CategoryBudgetGoal
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int CategoryId { get; private set; }
    public decimal MonthlyLimit { get; private set; }
    public GoalPriority Priority { get; private set; }
    public bool Active { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }

    protected CategoryBudgetGoal() { }

    public CategoryBudgetGoal(
        int userId,
        int categoryId,
        decimal monthlyLimit,
        GoalPriority priority)
    {
        if (monthlyLimit <= 0)
            throw new DomainException(MessageKeys.InvalidGoalAmount);

        UserId = userId;
        CategoryId = categoryId;
        MonthlyLimit = monthlyLimit;
        Priority = priority;
        Active = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateLimit(decimal monthlyLimit)
    {
        if (monthlyLimit <= 0)
            throw new DomainException(MessageKeys.InvalidGoalAmount);

        MonthlyLimit = monthlyLimit;
    }

    public void UpdatePriority(GoalPriority priority)
    {
        Priority = priority;
    }

    public void Deactivate()
    {
        Active = false;
        DeactivatedAt = DateTime.UtcNow;
    }
}
