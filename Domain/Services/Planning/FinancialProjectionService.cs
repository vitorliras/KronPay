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

        var opening = parameters.InitialBalance;
        var optOpening = parameters.InitialBalance;
        var pessOpening = parameters.InitialBalance;

        for (var i = 0; i < horizon; i++)
        {
            var bucket = firstMonth.AddMonths(i);

            var monthFlows = flowList.Where(f =>
                f.CompetenceDate.Year == bucket.Year &&
                f.CompetenceDate.Month == bucket.Month);

            decimal inflows = 0m, outflows = 0m, committedNet = 0m, estimatedNet = 0m, estimatedExposure = 0m;

            foreach (var flow in monthFlows)
            {
                if (flow.Direction == FlowDirection.Inflow)
                    inflows += flow.Amount;
                else
                    outflows += flow.Amount;

                if (flow.IsCommitted)
                    committedNet += flow.SignedAmount;
                else
                {
                    estimatedNet += flow.SignedAmount;
                    estimatedExposure += flow.Amount;
                }
            }

            var net = inflows - outflows;
            var band = parameters.EstimateSpreadRate * estimatedExposure;

            var closing = opening + net;
            var optClosing = optOpening + net + band;
            var pessClosing = pessOpening + net - band;

            months.Add(new ProjectionMonth(
                bucket.Year,
                bucket.Month,
                opening,
                inflows,
                outflows,
                closing,
                committedNet,
                estimatedNet,
                optClosing,
                pessClosing));

            opening = closing;
            optOpening = optClosing;
            pessOpening = pessClosing;
        }

        return new FinancialProjection(months);
    }
}
