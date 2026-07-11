using Application.DataRetention.Targets;
using Domain.Entities.Planning;
using Domain.Interfaces.Planning;
using Moq;
using Shouldly;

namespace Tests.Application.DataRetention.Targets;

public class PlannedCommitmentRetentionPurgeTargetTests
{
    [Fact]
    public async Task Apaga_compromissos_desativados_antes_do_corte()
    {
        var cutoff = new DateTime(2026, 7, 1);
        var commitment = CreateDeactivatedCommitment();

        var repository = new Mock<IPlannedCommitmentRepository>();
        repository.Setup(r => r.GetDeactivatedOlderThanAsync(cutoff))
            .ReturnsAsync(new List<PlannedCommitment> { commitment });

        var sut = new PlannedCommitmentRetentionPurgeTarget(repository.Object);

        var removed = await sut.PurgeAsync(cutoff);

        removed.ShouldBe(1);
        repository.Verify(r => r.DeleteRangeAsync(It.Is<IEnumerable<PlannedCommitment>>(c => c.Contains(commitment))), Times.Once);
    }

    [Fact]
    public async Task Nao_apaga_nada_quando_nao_ha_compromissos_desativados()
    {
        var cutoff = new DateTime(2026, 7, 1);

        var repository = new Mock<IPlannedCommitmentRepository>();
        repository.Setup(r => r.GetDeactivatedOlderThanAsync(cutoff))
            .ReturnsAsync(new List<PlannedCommitment>());

        var sut = new PlannedCommitmentRetentionPurgeTarget(repository.Object);

        var removed = await sut.PurgeAsync(cutoff);

        removed.ShouldBe(0);
        repository.Verify(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<PlannedCommitment>>()), Times.Never);
    }

    private static PlannedCommitment CreateDeactivatedCommitment()
    {
        var commitment = new PlannedCommitment(1, "Aluguel", 2000m, "O", "M", new DateTime(2026, 1, 1));
        commitment.Deactivate();
        return commitment;
    }
}
