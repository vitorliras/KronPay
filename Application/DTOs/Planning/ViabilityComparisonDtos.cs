namespace Application.DTOs.Planning;

public sealed record MonthlyViabilityComparisonRequest(
    decimal Amount,
    bool Installment,
    short InstallmentsCount,
    int? CreditCardId,
    int? HorizonMonths,
    decimal? SafetyReserve);

public sealed record MonthViabilityResponse(
    int Year,
    int Month,
    int Score,
    string Verdict);

public sealed record MonthlyViabilityComparisonResponse(
    IReadOnlyList<MonthViabilityResponse> Months);
