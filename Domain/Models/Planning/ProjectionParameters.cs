namespace Domain.Models.Planning;

/// <summary>
/// Parâmetros da projeção. <see cref="EstimateSpreadRate"/> define a largura da banda
/// de confiança aplicada apenas à parcela estimada de cada mês (0 = sem banda).
/// </summary>
public sealed record ProjectionParameters(
    DateTime ReferenceDate,
    int HorizonMonths,
    decimal InitialBalance,
    decimal SafetyReserve = 0m,
    decimal EstimateSpreadRate = 0.15m);
