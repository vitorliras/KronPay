using Application.DataRetention.Targets;
using Domain.Entities.Configuration;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Domain.Interfaces.Transactions;
using Moq;
using Shouldly;

namespace Tests.Application.DataRetention.Targets;

public class CategoryItemRetentionPurgeTargetTests
{
    [Fact]
    public async Task Apaga_subcategoria_sem_transacao_ou_compra_vinculada()
    {
        var cutoff = new DateTime(2026, 7, 1);
        var item = CreateDeactivatedCategoryItem();

        var categoryItems = new Mock<ICategoryItemRepository>();
        categoryItems.Setup(r => r.GetDeactivatedOlderThanAsync(cutoff))
            .ReturnsAsync(new List<CategoryItem> { item });

        var transactions = new Mock<ITransactionRepository>();
        transactions.Setup(r => r.ExistsByCategoryItemIdAsync(item.Id)).ReturnsAsync(false);

        var cardPurchases = new Mock<ICardPurchaseRepository>();
        cardPurchases.Setup(r => r.ExistsByCategoryItemIdAsync(item.Id)).ReturnsAsync(false);

        var sut = new CategoryItemRetentionPurgeTarget(categoryItems.Object, transactions.Object, cardPurchases.Object);

        var removed = await sut.PurgeAsync(cutoff);

        removed.ShouldBe(1);
        categoryItems.Verify(r => r.DeleteRangeAsync(It.Is<IEnumerable<CategoryItem>>(i => i.Contains(item))), Times.Once);
    }

    [Fact]
    public async Task Mantem_subcategoria_ainda_referenciada_por_transacao()
    {
        var cutoff = new DateTime(2026, 7, 1);
        var item = CreateDeactivatedCategoryItem();

        var categoryItems = new Mock<ICategoryItemRepository>();
        categoryItems.Setup(r => r.GetDeactivatedOlderThanAsync(cutoff))
            .ReturnsAsync(new List<CategoryItem> { item });

        var transactions = new Mock<ITransactionRepository>();
        transactions.Setup(r => r.ExistsByCategoryItemIdAsync(item.Id)).ReturnsAsync(true);

        var cardPurchases = new Mock<ICardPurchaseRepository>();
        cardPurchases.Setup(r => r.ExistsByCategoryItemIdAsync(item.Id)).ReturnsAsync(false);

        var sut = new CategoryItemRetentionPurgeTarget(categoryItems.Object, transactions.Object, cardPurchases.Object);

        var removed = await sut.PurgeAsync(cutoff);

        removed.ShouldBe(0);
        categoryItems.Verify(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<CategoryItem>>()), Times.Never);
    }

    [Fact]
    public async Task Mantem_subcategoria_ainda_referenciada_por_compra()
    {
        var cutoff = new DateTime(2026, 7, 1);
        var item = CreateDeactivatedCategoryItem();

        var categoryItems = new Mock<ICategoryItemRepository>();
        categoryItems.Setup(r => r.GetDeactivatedOlderThanAsync(cutoff))
            .ReturnsAsync(new List<CategoryItem> { item });

        var transactions = new Mock<ITransactionRepository>();
        transactions.Setup(r => r.ExistsByCategoryItemIdAsync(item.Id)).ReturnsAsync(false);

        var cardPurchases = new Mock<ICardPurchaseRepository>();
        cardPurchases.Setup(r => r.ExistsByCategoryItemIdAsync(item.Id)).ReturnsAsync(true);

        var sut = new CategoryItemRetentionPurgeTarget(categoryItems.Object, transactions.Object, cardPurchases.Object);

        var removed = await sut.PurgeAsync(cutoff);

        removed.ShouldBe(0);
        categoryItems.Verify(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<CategoryItem>>()), Times.Never);
    }

    private static CategoryItem CreateDeactivatedCategoryItem()
    {
        var item = new CategoryItem(1, "Lanches");
        item.Deactivate();
        return item;
    }
}
