using Domain.Enums.Goals;

namespace Application.DTOs.Goals;

public sealed record CreateCategoryBudgetGoalRequest(
    int CategoryId,
    decimal MonthlyLimit,
    GoalPriority Priority);
