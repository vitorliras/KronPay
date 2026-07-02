using Domain.Enums.Planning;
using Domain.Models.Planning;

namespace Domain.Services.Planning;

public sealed class FinancialProjectionService : IFinancialProjectionService
{
    public FinancialProjection Project(IEnumerable<FinancialFlow> flows, ProjectionParameters parameters)
    {
        var flowList = flows as IReadOnlyList<FinancialFlow> ?? flows.ToList();
        var horizon = Math.Max(parameters.HorizonMonths, 0);
        var firstMonth = new DateTime(parameters.ReferenceDate.Year, parameters.ReferenceDate.Month, 1);

        var months = new List<ProjectionMonth>(horizon);

        var predictedOpening = parameters.InitialBalance;
        var probableOpening = parameters.InitialBalance;

        for (var i = 0; i < horizon; i++)
        {
            var bucket = firstMonth.AddMonths(i);

            var monthFlows = flowList.Where(f =>
                f.CompetenceDate.Year == bucket.Year &&
                f.CompetenceDate.Month == bucket.Month);

            decimal inflows = 0m, committedOutflow = 0m, estimatedOutflow = 0m;

            foreach (var flow in monthFlows)
            {
                if (flow.Direction == FlowDirection.Inflow)
                {
                    inflows += flow.Amount;
                    continue;
                }

                if (flow.IsCommitted)
                    committedOutflow += flow.Amount;
                else
                    estimatedOutflow += flow.Amount;
            }

            var predictedOutflow = committedOutflow;
            var probableOutflow = committedOutflow + estimatedOutflow;

            var predictedClosing = predictedOpening + inflows - predictedOutflow;
            var probableClosing = probableOpening + inflows - probableOutflow;

            months.Add(new ProjectionMonth(
                bucket.Year,
                bucket.Month,
                probableOpening,
                inflows,
                predictedOutflow,
                probableOutflow,
                predictedClosing,
                probableClosing));

            predictedOpening = predictedClosing;
            probableOpening = probableClosing;
        }

        return new FinancialProjection(months);
    }
}
