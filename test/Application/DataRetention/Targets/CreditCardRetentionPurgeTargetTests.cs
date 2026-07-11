using Application.DataRetention.Targets;
using Domain.Entities.Card;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Moq;
using Shouldly;

namespace Tests.Application.DataRetention.Targets;

public class CreditCardRetentionPurgeTargetTests
{
    [Fact]
    public async Task Apaga_cartao_sem_compras_ou_faturas_vinculadas()
    {
        var cutoff = new DateTime(2026, 7, 1);
        var card = CreateDeactivatedCreditCard();

        var creditCards = new Mock<ICreditCardRepository>();
        creditCards.Setup(r => r.GetDeactivatedOlderThanAsync(cutoff))
            .ReturnsAsync(new List<CreditCard> { card });

        var cardPurchases = new Mock<ICardPurchaseRepository>();
        cardPurchases.Setup(r => r.ExistsByCreditCardIdAsync(card.Id)).ReturnsAsync(false);

        var cardInvoices = new Mock<ICardInvoiceRepository>();
        cardInvoices.Setup(r => r.ExistsByCreditCardIdAsync(card.Id)).ReturnsAsync(false);

        var sut = new CreditCardRetentionPurgeTarget(creditCards.Object, cardPurchases.Object, cardInvoices.Object);

        var removed = await sut.PurgeAsync(cutoff);

        removed.ShouldBe(1);
        creditCards.Verify(r => r.DeleteRangeAsync(It.Is<IEnumerable<CreditCard>>(c => c.Contains(card))), Times.Once);
    }

    [Fact]
    public async Task Mantem_cartao_ainda_referenciado_por_fatura()
    {
        var cutoff = new DateTime(2026, 7, 1);
        var card = CreateDeactivatedCreditCard();

        var creditCards = new Mock<ICreditCardRepository>();
        creditCards.Setup(r => r.GetDeactivatedOlderThanAsync(cutoff))
            .ReturnsAsync(new List<CreditCard> { card });

        var cardPurchases = new Mock<ICardPurchaseRepository>();
        cardPurchases.Setup(r => r.ExistsByCreditCardIdAsync(card.Id)).ReturnsAsync(false);

        var cardInvoices = new Mock<ICardInvoiceRepository>();
        cardInvoices.Setup(r => r.ExistsByCreditCardIdAsync(card.Id)).ReturnsAsync(true);

        var sut = new CreditCardRetentionPurgeTarget(creditCards.Object, cardPurchases.Object, cardInvoices.Object);

        var removed = await sut.PurgeAsync(cutoff);

        removed.ShouldBe(0);
        creditCards.Verify(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<CreditCard>>()), Times.Never);
    }

    private static CreditCard CreateDeactivatedCreditCard()
    {
        var card = new CreditCard(1, 1, 1, "Nubank", 10, 5, 5000m);
        card.Deactivate();
        return card;
    }
}
