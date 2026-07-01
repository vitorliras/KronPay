using Domain.Models.Planning;

namespace Application.Planning.Flows;

public interface IFinancialFlowSource
{
    Task<IEnumerable<FinancialFlow>> GetFlowsAsync(int userId, DateTime from, DateTime to);
}
