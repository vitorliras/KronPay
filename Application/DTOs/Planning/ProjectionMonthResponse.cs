namespace Application.DTOs.Planning;

public sealed record ProjectionMonthResponse(
    int Year,
    int Month,
    decimal OpeningBalance,
    decimal Inflows,
    decimal PredictedOutflow,
    decimal ProbableOutflow,
    decimal PredictedClosing,
    decimal ProbableClosing);
