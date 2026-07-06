using Domain.Enums.Goals;

namespace Application.DTOs.Goals;

public sealed record CategoryBudgetGoalResponse(
    int Id,
    int CategoryId,
    decimal MonthlyLimit,
    GoalPriority Priority,
    bool Active,
    decimal CurrentPeriodSpent);
