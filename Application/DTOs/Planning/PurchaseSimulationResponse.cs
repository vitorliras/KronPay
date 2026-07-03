namespace Application.DTOs.Planning;

public sealed record PurchaseSimulationResponse(
    decimal BaseFinalBalance,
    decimal SimulatedFinalBalance,
    decimal SafetyReserve,
    int? FirstNegativeYear,
    int? FirstNegativeMonth,
    ViabilityResponse Viability,
    IReadOnlyList<ProjectionMonthResponse> Months);
