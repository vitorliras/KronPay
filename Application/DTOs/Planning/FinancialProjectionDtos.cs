namespace Application.DTOs.Planning;

public sealed record GetFinancialProjectionRequest(
    int? HorizonMonths,
    decimal? SafetyReserve);

public sealed record FinancialProjectionResponse(
    decimal InitialBalance,
    decimal FinalBalance,
    int? FirstNegativeYear,
    int? FirstNegativeMonth,
    ViabilityResponse Viability,
    IReadOnlyList<ProjectionMonthResponse> Months);
