namespace Domain.Services.Planning;

/// <summary>
/// Serviço de domínio (puro) que estima o gasto variável mensal futuro a partir do
/// histórico mensal (média móvel ponderada + tendência). Trocável por implementação melhor.
/// </summary>
public interface IVariableSpendingEstimator
{
    /// <param name="monthlyHistory">Totais mensais do histórico, do mais antigo ao mais recente.</param>
    /// <param name="horizonMonths">Quantidade de meses futuros a estimar.</param>
    /// <returns>Valor estimado por mês futuro (mesmo tamanho de <paramref name="horizonMonths"/>).</returns>
    IReadOnlyList<decimal> Estimate(IReadOnlyList<decimal> monthlyHistory, int horizonMonths);
}
