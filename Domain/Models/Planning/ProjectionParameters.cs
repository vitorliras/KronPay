namespace Domain.Models.Planning;

public sealed record ProjectionParameters(
    DateTime ReferenceDate,
    int HorizonMonths,
    decimal InitialBalance,
    decimal SafetyReserve = 0m,
    decimal EstimateSpreadRate = 0.15m);
