using Application.Planning.Flows;
using Domain.Enums.Planning;
using Domain.Models.Planning;
using Moq;
using Shouldly;

namespace Tests.Application.Planning.Flows;

public class FinancialFlowAggregatorTests
{
    [Fact]
    public async Task Concatena_os_fluxos_de_todas_as_fontes()
    {
        var flowA = new FinancialFlow(new DateTime(2026, 6, 1), FlowDirection.Inflow, 100m, ConfidenceLevel.High, FlowOrigin.Transaction);
        var flowB = new FinancialFlow(new DateTime(2026, 6, 2), FlowDirection.Outflow, 50m, ConfidenceLevel.High, FlowOrigin.CardInvoice);

        var sourceA = new Mock<IFinancialFlowSource>();
        sourceA.Setup(s => s.GetFlowsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new[] { flowA });

        var sourceB = new Mock<IFinancialFlowSource>();
        sourceB.Setup(s => s.GetFlowsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new[] { flowB });

        var sut = new FinancialFlowAggregator(new[] { sourceA.Object, sourceB.Object });

        var flows = await sut.CollectAsync(1, new DateTime(2026, 6, 1), new DateTime(2026, 8, 31));

        flows.Count.ShouldBe(2);
        flows.ShouldContain(flowA);
        flows.ShouldContain(flowB);
    }
}
