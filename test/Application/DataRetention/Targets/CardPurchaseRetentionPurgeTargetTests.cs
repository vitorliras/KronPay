using Application.DataRetention.Targets;
using Domain.Entities.Card;
using Domain.Interfaces.Card;
using Moq;
using Shouldly;

namespace Tests.Application.DataRetention.Targets;

public class CardPurchaseRetentionPurgeTargetTests
{
    [Fact]
    public async Task Apaga_compra_sem_parcelas_vinculadas()
    {
        var cutoff = new DateTime(2026, 7, 1);
        var purchase = CreateDeactivatedPurchase();

        var repository = new Mock<ICardPurchaseRepository>();
        repository.Setup(r => r.GetDeactivatedOlderThanAsync(cutoff))
            .ReturnsAsync(new List<CardPurchase> { purchase });
        repository.Setup(r => r.ExistsInstallmentByCardPurchaseIdAsync(purchase.Id)).ReturnsAsync(false);

        var sut = new CardPurchaseRetentionPurgeTarget(repository.Object);

        var removed = await sut.PurgeAsync(cutoff);

        removed.ShouldBe(1);
        repository.Verify(r => r.DeleteRangeAsync(It.Is<IEnumerable<CardPurchase>>(p => p.Contains(purchase))), Times.Once);
    }

    [Fact]
    public async Task Mantem_compra_ainda_referenciada_por_parcela()
    {
        var cutoff = new DateTime(2026, 7, 1);
        var purchase = CreateDeactivatedPurchase();

        var repository = new Mock<ICardPurchaseRepository>();
        repository.Setup(r => r.GetDeactivatedOlderThanAsync(cutoff))
            .ReturnsAsync(new List<CardPurchase> { purchase });
        repository.Setup(r => r.ExistsInstallmentByCardPurchaseIdAsync(purchase.Id)).ReturnsAsync(true);

        var sut = new CardPurchaseRetentionPurgeTarget(repository.Object);

        var removed = await sut.PurgeAsync(cutoff);

        removed.ShouldBe(0);
        repository.Verify(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<CardPurchase>>()), Times.Never);
    }

    private static CardPurchase CreateDeactivatedPurchase()
    {
        var purchase = new CardPurchase(1, 1, "Compra teste", 100m, new DateTime(2026, 1, 1), 1, null, null, "manual");
        purchase.Deactivate();
        return purchase;
    }
}
