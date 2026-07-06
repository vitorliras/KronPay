using Domain.Enums.Goals;

namespace Application.DTOs.Goals;

public sealed record UpdateFinancialGoalRequest(
    int Id,
    string Description,
    decimal TargetAmount,
    DateTime TargetDate,
    GoalPriority Priority);
