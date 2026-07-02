namespace Domain.Models.Planning;

public sealed record ProjectionMonth(
    int Year,
    int Month,
    decimal OpeningBalance,
    decimal Inflows,
    decimal PredictedOutflow,
    decimal ProbableOutflow,
    decimal PredictedClosing,
    decimal ProbableClosing);
