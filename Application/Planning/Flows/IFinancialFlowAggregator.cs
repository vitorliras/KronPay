using Domain.Models.Planning;

namespace Application.Planning.Flows;

public interface IFinancialFlowAggregator
{
    Task<IReadOnlyList<FinancialFlow>> CollectAsync(int userId, DateTime from, DateTime to);
}
