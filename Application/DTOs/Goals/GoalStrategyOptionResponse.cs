using Domain.Enums.Goals;

namespace Application.DTOs.Goals;

public sealed record GoalStrategyOptionResponse(
    GoalStrategyLabel Label,
    decimal MonthlyContribution,
    DateTime? ProjectedCompletionDate,
    decimal SafetyReserveImpact);
