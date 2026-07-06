using Domain.Enums.Goals;

namespace Application.DTOs.Goals;

public sealed record ContributionResponse(decimal CurrentAmount, FinancialGoalStatus Status);
