namespace Application.DTOs.Planning;

public sealed record FinancialProjectionResponse(
    decimal InitialBalance,
    decimal FinalBalance,
    decimal SafetyReserve,
    int? FirstNegativeYear,
    int? FirstNegativeMonth,
    ViabilityResponse Viability,
    IReadOnlyList<ProjectionMonthResponse> Months);
