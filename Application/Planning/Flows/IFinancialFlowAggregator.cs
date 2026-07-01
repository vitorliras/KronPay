using Domain.Models.Planning;

namespace Application.Planning.Flows;

/// <summary>Reúne os fluxos de todas as <see cref="IFinancialFlowSource"/> registradas.</summary>
public interface IFinancialFlowAggregator
{
    Task<IReadOnlyList<FinancialFlow>> CollectAsync(int userId, DateTime from, DateTime to);
}
