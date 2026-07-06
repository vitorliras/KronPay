using Domain.Enums.Goals;

namespace Application.DTOs.Goals;

public sealed record CreateFinancialGoalRequest(
    string Description,
    decimal TargetAmount,
    DateTime TargetDate,
    GoalPriority Priority);
