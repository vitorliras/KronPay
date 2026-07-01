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

public sealed record ViabilityFindingResponse(
    string Rule,
    string Status,
    int Penalty,
    bool IsVeto,
    string MessageKey,
    int? Year,
    int? Month);

public sealed record ViabilityResponse(
    int Score,
    string Verdict,
    IReadOnlyList<ViabilityFindingResponse> Findings);
