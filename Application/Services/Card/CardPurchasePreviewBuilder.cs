using Application.Abstractions.AI;
using Application.DTOs.Card.CardPurchases;
using Application.Services.Transactions;
using Domain.Entities.Configuration;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Domain.Services.Transactions;
using Shared.AI;

namespace Application.Services.Card;

public sealed class CardPurchasePreviewBuilder
{
    private const string ExpenseType = "E";

    private readonly ImportedTransactionClassificationService _classificationService;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICardPurchaseRepository _purchaseRepository;

    public CardPurchasePreviewBuilder(
        ImportedTransactionClassificationService classificationService,
        ICategoryRepository categoryRepository,
        ICardPurchaseRepository purchaseRepository)
    {
        _classificationService = classificationService;
        _categoryRepository = categoryRepository;
        _purchaseRepository = purchaseRepository;
    }

    public async Task<CardPurchasePreviewResponse> BuildAsync(
        int userId,
        int creditCardId,
        IReadOnlyList<ExtractedCardPurchase> purchases,
        int skipped,
        bool useAi)
    {
        var categories = (await _categoryRepository.GetAllAsync(userId)).ToList();

        var imported = purchases
            .Select(p => new ImportedTransactionResponse(
                p.Date, p.Amount, p.Description, ExpenseType, "P", 1, null, null, null, null))
            .ToList();

        var classified = await _classificationService.ClassifyAsync(userId, imported, categories, useAi);
        var duplicates = await LoadDuplicateKeysAsync(userId, creditCardId, purchases);

        var items = new List<CardPurchasePreviewItemResponse>(purchases.Count);

        for (var index = 0; index < purchases.Count; index++)
        {
            var purchase = purchases[index];
            var suggestion = classified[index] ?? imported[index];
            var validCategory = IsExpenseCategory(categories, suggestion);

            items.Add(new CardPurchasePreviewItemResponse(
                purchase.Date,
                purchase.Description,
                purchase.Amount,
                purchase.InstallmentsCount,
                validCategory ? suggestion.CategoryId : null,
                validCategory ? suggestion.CategoryItemId : null,
                validCategory ? suggestion.ClassificationSource : null,
                duplicates.Contains(KeyOf(purchase.Date, purchase.Amount, purchase.Description))));
        }

        return new CardPurchasePreviewResponse(purchases.Count + skipped, skipped, items);
    }

    private async Task<HashSet<(DateTime Date, decimal Amount, string Description)>> LoadDuplicateKeysAsync(
        int userId,
        int creditCardId,
        IReadOnlyList<ExtractedCardPurchase> purchases)
    {
        if (purchases.Count == 0)
            return new();

        var existing = await _purchaseRepository.GetActiveByCardAndPeriodAsync(
            creditCardId,
            userId,
            purchases.Min(p => p.Date).Date,
            purchases.Max(p => p.Date).Date);

        return existing
            .Select(e => KeyOf(e.PurchaseDate, e.TotalAmount, e.Description))
            .ToHashSet();
    }

    private static bool IsExpenseCategory(IEnumerable<Category> categories, ImportedTransactionResponse item) =>
        item.CategoryId is { } categoryId
        && categories.Any(c => c.Id == categoryId && c.CodTypeTransaction == ExpenseType);

    private static (DateTime Date, decimal Amount, string Description) KeyOf(
        DateTime date,
        decimal amount,
        string description)
    {
        var normalized = TransactionDescriptionNormalizer.Normalize(description);

        return (date.Date, amount, normalized.Length > 0 ? normalized : description.Trim().ToLowerInvariant());
    }
}
