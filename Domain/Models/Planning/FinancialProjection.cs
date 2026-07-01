namespace Domain.Models.Planning;

/// <summary>Resultado de um mês da projeção (saldo encadeado + breakdown de confiança + banda).</summary>
public sealed record ProjectionMonth(
    int Year,
    int Month,
    decimal OpeningBalance,
    decimal Inflows,
    decimal Outflows,
    decimal ClosingBalance,
    decimal CommittedNet,
    decimal EstimatedNet,
    decimal OptimisticClosing,
    decimal PessimisticClosing);

/// <summary>Projeção financeira completa do horizonte.</summary>
public sealed record FinancialProjection(IReadOnlyList<ProjectionMonth> Months)
{
    public decimal FinalBalance => Months.Count > 0 ? Months[^1].ClosingBalance : 0m;

    /// <summary>Primeiro mês em que o cenário pessimista fica negativo (ou null se nunca).</summary>
    public ProjectionMonth? FirstNegativeMonth
        => Months.FirstOrDefault(m => m.PessimisticClosing < 0);
}
