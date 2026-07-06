using Domain.Enums.Goals;

namespace Application.DTOs.Goals;

public sealed record FinancialGoalResponse(
    int Id,
    string Description,
    decimal TargetAmount,
    decimal CurrentAmount,
    DateTime TargetDate,
    GoalPriority Priority,
    FinancialGoalStatus Status,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    int? PreviousAttemptGoalId);
