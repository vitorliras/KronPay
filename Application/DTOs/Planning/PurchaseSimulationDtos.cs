namespace Application.DTOs.Planning;

public sealed record SimulatePurchaseRequest(
    decimal Amount,
    DateTime PurchaseDate,
    bool Installment,
    short InstallmentsCount,
    int? CreditCardId,
    int? HorizonMonths,
    decimal? SafetyReserve);

public sealed record PurchaseSimulationResponse(
    decimal BaseFinalBalance,
    decimal SimulatedFinalBalance,
    int? FirstNegativeYear,
    int? FirstNegativeMonth,
    ViabilityResponse Viability,
    IReadOnlyList<ProjectionMonthResponse> Months);
