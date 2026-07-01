namespace Application.DTOs.Planning;

public sealed record MonthlyViabilityComparisonResponse(
    IReadOnlyList<MonthViabilityResponse> Months);
