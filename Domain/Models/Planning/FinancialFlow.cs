using Domain.Enums.Planning;

namespace Domain.Models.Planning;

/// <summary>
/// Modelo normalizado (em memória, não persistido) de qualquer entrada/saída financeira.
/// É a "moeda comum" que o motor de projeção consome, desacoplado das fontes reais.
/// </summary>
public sealed record FinancialFlow(
    DateTime CompetenceDate,
    FlowDirection Direction,
    decimal Amount,
    ConfidenceLevel Confidence,
    FlowOrigin Origin,
    string? Description = null)
{
    /// <summary>Valor com sinal: positivo para entrada, negativo para saída.</summary>
    public decimal SignedAmount => Direction == FlowDirection.Inflow ? Amount : -Amount;

    public bool IsCommitted => Confidence == ConfidenceLevel.High;
}
