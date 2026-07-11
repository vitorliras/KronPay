using Application.DataRetention.Targets;
using Domain.Entities.Configuration;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Domain.Interfaces.Goals;
using Domain.Interfaces.Planning;
using Domain.Interfaces.Transactions;
using Moq;
using Shouldly;

namespace Tests.Application.DataRetention.Targets;

public class CategoryRetentionPurgeTargetTests
{
    [Fact]
    public async Task Apaga_categoria_sem_nenhuma_referencia()
    {
        var cutoff = new DateTime(2026, 7, 1);
        var category = CreateDeactivatedCategory();

        var (categories, transactions, cardPurchases, categoryItems, plannedCommitments, categoryBudgetGoals) =
            CreateMocksWithNoReferences(category, cutoff);

        var sut = new CategoryRetentionPurgeTarget(
            categories.Object, transactions.Object, cardPurchases.Object,
            categoryItems.Object, plannedCommitments.Object, categoryBudgetGoals.Object);

        var removed = await sut.PurgeAsync(cutoff);

        removed.ShouldBe(1);
        categories.Verify(r => r.DeleteRangeAsync(It.Is<IEnumerable<Category>>(c => c.Contains(category))), Times.Once);
    }

    [Fact]
    public async Task Mantem_categoria_ainda_referenciada_por_meta_de_orcamento()
    {
        var cutoff = new DateTime(2026, 7, 1);
        var category = CreateDeactivatedCategory();

        var (categories, transactions, cardPurchases, categoryItems, plannedCommitments, categoryBudgetGoals) =
            CreateMocksWithNoReferences(category, cutoff);

        categoryBudgetGoals.Setup(r => r.ExistsByCategoryIdAsync(category.Id)).ReturnsAsync(true);

        var sut = new CategoryRetentionPurgeTarget(
            categories.Object, transactions.Object, cardPurchases.Object,
            categoryItems.Object, plannedCommitments.Object, categoryBudgetGoals.Object);

        var removed = await sut.PurgeAsync(cutoff);

        removed.ShouldBe(0);
        categories.Verify(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<Category>>()), Times.Never);
    }

    [Fact]
    public async Task Mantem_categoria_ainda_referenciada_por_transacao()
    {
        var cutoff = new DateTime(2026, 7, 1);
        var category = CreateDeactivatedCategory();

        var (categories, transactions, cardPurchases, categoryItems, plannedCommitments, categoryBudgetGoals) =
            CreateMocksWithNoReferences(category, cutoff);

        transactions.Setup(r => r.ExistsByCategoryIdAsync(category.Id)).ReturnsAsync(true);

        var sut = new CategoryRetentionPurgeTarget(
            categories.Object, transactions.Object, cardPurchases.Object,
            categoryItems.Object, plannedCommitments.Object, categoryBudgetGoals.Object);

        var removed = await sut.PurgeAsync(cutoff);

        removed.ShouldBe(0);
        categories.Verify(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<Category>>()), Times.Never);
    }

    [Fact]
    public async Task Mantem_categoria_ainda_referenciada_por_compra_no_cartao()
    {
        var cutoff = new DateTime(2026, 7, 1);
        var category = CreateDeactivatedCategory();

        var (categories, transactions, cardPurchases, categoryItems, plannedCommitments, categoryBudgetGoals) =
            CreateMocksWithNoReferences(category, cutoff);

        cardPurchases.Setup(r => r.ExistsByCategoryIdAsync(category.Id)).ReturnsAsync(true);

        var sut = new CategoryRetentionPurgeTarget(
            categories.Object, transactions.Object, cardPurchases.Object,
            categoryItems.Object, plannedCommitments.Object, categoryBudgetGoals.Object);

        var removed = await sut.PurgeAsync(cutoff);

        removed.ShouldBe(0);
        categories.Verify(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<Category>>()), Times.Never);
    }

    [Fact]
    public async Task Mantem_categoria_ainda_referenciada_por_subcategoria()
    {
        var cutoff = new DateTime(2026, 7, 1);
        var category = CreateDeactivatedCategory();

        var (categories, transactions, cardPurchases, categoryItems, plannedCommitments, categoryBudgetGoals) =
            CreateMocksWithNoReferences(category, cutoff);

        categoryItems.Setup(r => r.ExistsByCategoryIdAsync(category.Id)).ReturnsAsync(true);

        var sut = new CategoryRetentionPurgeTarget(
            categories.Object, transactions.Object, cardPurchases.Object,
            categoryItems.Object, plannedCommitments.Object, categoryBudgetGoals.Object);

        var removed = await sut.PurgeAsync(cutoff);

        removed.ShouldBe(0);
        categories.Verify(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<Category>>()), Times.Never);
    }

    [Fact]
    public async Task Mantem_categoria_ainda_referenciada_por_compromisso_planejado()
    {
        var cutoff = new DateTime(2026, 7, 1);
        var category = CreateDeactivatedCategory();

        var (categories, transactions, cardPurchases, categoryItems, plannedCommitments, categoryBudgetGoals) =
            CreateMocksWithNoReferences(category, cutoff);

        plannedCommitments.Setup(r => r.ExistsByCategoryIdAsync(category.Id)).ReturnsAsync(true);

        var sut = new CategoryRetentionPurgeTarget(
            categories.Object, transactions.Object, cardPurchases.Object,
            categoryItems.Object, plannedCommitments.Object, categoryBudgetGoals.Object);

        var removed = await sut.PurgeAsync(cutoff);

        removed.ShouldBe(0);
        categories.Verify(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<Category>>()), Times.Never);
    }

    private static Category CreateDeactivatedCategory()
    {
        var category = new Category(1, "Alimentação", "E");
        category.Deactivate();
        return category;
    }

    private static (
        Mock<ICategoryRepository> categories,
        Mock<ITransactionRepository> transactions,
        Mock<ICardPurchaseRepository> cardPurchases,
        Mock<ICategoryItemRepository> categoryItems,
        Mock<IPlannedCommitmentRepository> plannedCommitments,
        Mock<ICategoryBudgetGoalRepository> categoryBudgetGoals
        ) CreateMocksWithNoReferences(Category category, DateTime cutoff)
    {
        var categories = new Mock<ICategoryRepository>();
        categories.Setup(r => r.GetDeactivatedOlderThanAsync(cutoff))
            .ReturnsAsync(new List<Category> { category });

        var transactions = new Mock<ITransactionRepository>();
        transactions.Setup(r => r.ExistsByCategoryIdAsync(category.Id)).ReturnsAsync(false);

        var cardPurchases = new Mock<ICardPurchaseRepository>();
        cardPurchases.Setup(r => r.ExistsByCategoryIdAsync(category.Id)).ReturnsAsync(false);

        var categoryItems = new Mock<ICategoryItemRepository>();
        categoryItems.Setup(r => r.ExistsByCategoryIdAsync(category.Id)).ReturnsAsync(false);

        var plannedCommitments = new Mock<IPlannedCommitmentRepository>();
        plannedCommitments.Setup(r => r.ExistsByCategoryIdAsync(category.Id)).ReturnsAsync(false);

        var categoryBudgetGoals = new Mock<ICategoryBudgetGoalRepository>();
        categoryBudgetGoals.Setup(r => r.ExistsByCategoryIdAsync(category.Id)).ReturnsAsync(false);

        return (categories, transactions, cardPurchases, categoryItems, plannedCommitments, categoryBudgetGoals);
    }
}
