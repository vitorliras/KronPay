using Domain.Enums.Goals;

namespace Application.DTOs.Goals;

public sealed record UpdateCategoryBudgetGoalRequest(
    int Id,
    decimal MonthlyLimit,
    GoalPriority Priority);
