namespace Application.DTOs.Planning;

public sealed record ProjectionMonthResponse(
    int Year,
    int Month,
    decimal OpeningBalance,
    decimal Inflows,
    decimal Outflows,
    decimal ClosingBalance,
    decimal CommittedNet,
    decimal EstimatedNet,
    decimal OptimisticClosing,
    decimal PessimisticClosing);
