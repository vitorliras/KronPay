using Application.Configuration.DataRetention;
using Application.DataRetention;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;

namespace Tests.Application.DataRetention;

public class DataRetentionPurgeOrchestratorTests
{
    [Fact]
    public async Task Soma_o_total_removido_por_todos_os_targets_e_faz_commit()
    {
        var cutoffCaptured = new List<DateTime>();

        var targetA = new Mock<IRetentionPurgeTarget>();
        targetA.Setup(t => t.PurgeAsync(It.IsAny<DateTime>()))
            .Callback<DateTime>(cutoffCaptured.Add)
            .ReturnsAsync(2);

        var targetB = new Mock<IRetentionPurgeTarget>();
        targetB.Setup(t => t.PurgeAsync(It.IsAny<DateTime>()))
            .Callback<DateTime>(cutoffCaptured.Add)
            .ReturnsAsync(3);

        var uow = new Mock<IUnitOfWork>();
        uow.Setup(u => u.CommitAsync(null)).ReturnsAsync(true);

        var settings = Options.Create(new DataRetentionSettings { PurgeAfterDays = 30 });
        var logger = new Mock<ILogger<DataRetentionPurgeOrchestrator>>();

        var sut = new DataRetentionPurgeOrchestrator(
            new[] { targetA.Object, targetB.Object }, settings, uow.Object, logger.Object);

        var removed = await sut.RunAsync();

        removed.ShouldBe(5);
        uow.Verify(u => u.CommitAsync(null), Times.Once);
        cutoffCaptured.Count.ShouldBe(2);
        cutoffCaptured.ShouldAllBe(c => c == cutoffCaptured[0]);
    }
}
