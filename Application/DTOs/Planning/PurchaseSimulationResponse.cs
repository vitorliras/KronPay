namespace Application.DTOs.Planning;

public sealed record PurchaseSimulationResponse(
    decimal BaseFinalBalance,
    decimal SimulatedFinalBalance,
    int? FirstNegativeYear,
    int? FirstNegativeMonth,
    ViabilityResponse Viability,
    IReadOnlyList<ProjectionMonthResponse> Months);
