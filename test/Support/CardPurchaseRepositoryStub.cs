using Domain.Interfaces.Card;
using Domain.Services.Transactions;
using Moq;

namespace Tests.Support;

internal static class CardPurchaseRepositoryStub
{
    public static ICardPurchaseRepository Empty() => With();

    public static ICardPurchaseRepository With(params CategorizedTransactionHistoryItem[] history)
    {
        var mock = new Mock<ICardPurchaseRepository>();

        mock.Setup(r => r.GetCategorizedHistoryAsync(It.IsAny<int>()))
            .ReturnsAsync(history.ToList());

        return mock.Object;
    }
}
