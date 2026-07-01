using Domain.Models.Planning;

namespace Application.Planning.Flows;

public sealed class FinancialFlowAggregator : IFinancialFlowAggregator
{
    private readonly IEnumerable<IFinancialFlowSource> _sources;

    public FinancialFlowAggregator(IEnumerable<IFinancialFlowSource> sources)
    {
        _sources = sources;
    }

    public async Task<IReadOnlyList<FinancialFlow>> CollectAsync(int userId, DateTime from, DateTime to)
    {
        var flows = new List<FinancialFlow>();

        foreach (var source in _sources)
            flows.AddRange(await source.GetFlowsAsync(userId, from, to));

        return flows;
    }
}
