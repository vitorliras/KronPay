using Application.Planning.Flows;
using Domain.Entities.Planning;
using Domain.Enums.Planning;
using Domain.Interfaces.Planning;
using Moq;
using Shouldly;

namespace Tests.Application.Planning.Flows;

public class CommitmentFlowSourceTests
{
    private readonly Mock<IPlannedCommitmentRepository> _repo = new();
    private readonly CommitmentFlowSource _sut;

    public CommitmentFlowSourceTests() => _sut = new CommitmentFlowSource(_repo.Object);

    [Fact]
    public async Task Expande_compromisso_mensal_em_uma_saida_por_mes()
    {
        var from = new DateTime(2026, 6, 1);
        var to = new DateTime(2026, 8, 31);

        var commitments = new List<PlannedCommitment>
        {
            new(1, "Aluguel", 1500m, "O", "M", new DateTime(2026, 1, 5))
        };

        _repo.Setup(r => r.GetByUserAsync(It.IsAny<int>())).ReturnsAsync(commitments);

        var flows = (await _sut.GetFlowsAsync(1, from, to)).ToList();

        flows.Count.ShouldBe(3); // jun, jul, ago
        flows.ShouldAllBe(f =>
            f.Direction == FlowDirection.Outflow &&
            f.Amount == 1500m &&
            f.Confidence == ConfidenceLevel.High &&
            f.Origin == FlowOrigin.PlannedCommitment);
        flows[0].CompetenceDate.ShouldBe(new DateTime(2026, 6, 5));
    }

    [Fact]
    public async Task Compromisso_de_entrada_gera_inflow()
    {
        var from = new DateTime(2026, 6, 1);
        var to = new DateTime(2026, 6, 30);

        var commitments = new List<PlannedCommitment>
        {
            new(1, "Freelance", 800m, "I", "U", new DateTime(2026, 6, 15))
        };

        _repo.Setup(r => r.GetByUserAsync(It.IsAny<int>())).ReturnsAsync(commitments);

        var flows = (await _sut.GetFlowsAsync(1, from, to)).ToList();

        flows.ShouldHaveSingleItem();
        flows[0].Direction.ShouldBe(FlowDirection.Inflow);
    }
}
