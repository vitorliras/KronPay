namespace Application.DTOs.Planning;

public sealed record MonthlyViabilityComparisonRequest(
    decimal Amount,
    bool Installment,
    short InstallmentsCount,
    int? CreditCardId,
    int? HorizonMonths,
    decimal? SafetyReserve);
