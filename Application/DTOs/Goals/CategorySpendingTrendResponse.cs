using Domain.Enums.Goals;

namespace Application.DTOs.Goals;

public sealed record CategorySpendingTrendResponse(
    decimal CurrentPeriodSpent,
    decimal HistoricalAverage,
    SpendingTrendDirection Direction);
