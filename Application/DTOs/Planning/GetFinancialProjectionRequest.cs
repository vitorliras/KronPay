namespace Application.DTOs.Planning;

public sealed record GetFinancialProjectionRequest(
    int? HorizonMonths,
    decimal? SafetyReserve);
