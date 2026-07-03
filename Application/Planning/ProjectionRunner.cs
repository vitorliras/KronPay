using Application.Planning.Flows;
using Domain.Interfaces.Transactions;
using Domain.Models.Planning;
using Domain.Services.Planning;

namespace Application.Planning;

public sealed class ProjectionRunner : IProjectionRunner
{
    private readonly ITransactionRepository _transactions;
    private readonly IFinancialFlowAggregator _aggregator;
    private readonly IFinancialProjectionService _projectionService;
    private readonly ISafetyReserveCalculator _reserveCalculator;

    public ProjectionRunner(
        ITransactionRepository transactions,
        IFinancialFlowAggregator aggregator,
        IFinancialProjectionService projectionService,
        ISafetyReserveCalculator reserveCalculator)
    {
        _transactions = transactions;
        _aggregator = aggregator;
        _projectionService = projectionService;
        _reserveCalculator = reserveCalculator;
    }

    public async Task<ProjectionContext> RunAsync(
        int userId,
        DateTime referenceDate,
        int horizonMonths,
        decimal? safetyReserve,
        IEnumerable<FinancialFlow>? extraFlows = null)
    {
        var from = new DateTime(referenceDate.Year, referenceDate.Month, 1);
        var to = from.AddMonths(horizonMonths).AddDays(-1);

        var initialBalance = await ComputeInitialBalanceAsync(userId, referenceDate);

        var flows = (await _aggregator.CollectAsync(userId, from, to)).ToList();
        if (extraFlows is not null)
            flows.AddRange(extraFlows);

        var draftParameters = new ProjectionParameters(referenceDate, horizonMonths, initialBalance, 0m);
        var projection = _projectionService.Project(flows, draftParameters);

        var reserve = safetyReserve ?? _reserveCalculator.Calculate(projection);
        var parameters = draftParameters with { SafetyReserve = reserve };

        return new ProjectionContext(projection, parameters);
    }

    private async Task<decimal> ComputeInitialBalanceAsync(int userId, DateTime referenceDate)
    {
        //MELHORAR AQUI: criar um repositorio só para essa lista
        var transactions = await _transactions.GetAllTransactionAsync(userId);

        return transactions
            .Where(t => t.Status == "P" && t.TransactionDate.Date <= referenceDate.Date
                && (t.CodTypeTransaction == "I" || t.CodTypeTransaction == "E"))
            .Sum(t => t.CodTypeTransaction == "I" ? t.Amount : -t.Amount);
    }
}
