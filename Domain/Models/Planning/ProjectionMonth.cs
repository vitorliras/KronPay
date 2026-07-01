namespace Domain.Models.Planning;

public sealed record ProjectionMonth(
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
