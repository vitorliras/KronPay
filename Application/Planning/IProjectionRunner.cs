using Domain.Models.Planning;

namespace Application.Planning;

public interface IProjectionRunner
{
    Task<ProjectionContext> RunAsync(
        int userId,
        DateTime referenceDate,
        int horizonMonths,
        decimal? safetyReserve,
        IEnumerable<FinancialFlow>? extraFlows = null);
}
