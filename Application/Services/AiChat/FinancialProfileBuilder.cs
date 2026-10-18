using System.Globalization;
using System.Text;
using Application.Services.Spending;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Domain.Interfaces.Goals;
using Domain.Interfaces.Planning;

namespace Application.Services.AiChat;

public sealed class FinancialProfileBuilder
{
    private const int TopCategories = 5;
    private const int HistoryMonths = 6;
    private const int MaxCategoriesPerType = 40;
    private const int MaxSubcategoriesPerCategory = 15;

    private static readonly (string Code, string Label)[] CategoryTypeLabels =
    {
        ("E", "Expense"),
        ("I", "Income"),
        ("V", "Investment")
    };

    private readonly SpendingAggregator _aggregator;
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ICardInvoiceRepository _invoiceRepository;
    private readonly ICardPurchaseRepository _purchaseRepository;
    private readonly IFinancialGoalRepository _goalRepository;
    private readonly IPlannedCommitmentRepository _commitmentRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICategoryItemRepository _categoryItemRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;

    public FinancialProfileBuilder(
        SpendingAggregator aggregator,
        ICreditCardRepository creditCardRepository,
        ICardInvoiceRepository invoiceRepository,
        ICardPurchaseRepository purchaseRepository,
        IFinancialGoalRepository goalRepository,
        IPlannedCommitmentRepository commitmentRepository,
        ICategoryRepository categoryRepository,
        ICategoryItemRepository categoryItemRepository,
        IPaymentMethodRepository paymentMethodRepository)
    {
        _aggregator = aggregator;
        _creditCardRepository = creditCardRepository;
        _invoiceRepository = invoiceRepository;
        _purchaseRepository = purchaseRepository;
        _goalRepository = goalRepository;
        _commitmentRepository = commitmentRepository;
        _categoryRepository = categoryRepository;
        _categoryItemRepository = categoryItemRepository;
        _paymentMethodRepository = paymentMethodRepository;
    }

    public async Task<string> BuildAsync(int userId, DateTime today)
    {
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var sb = new StringBuilder();

        sb.AppendLine($"Today: {today:yyyy-MM-dd} ({today.ToString("dddd", CultureInfo.InvariantCulture)}). Currency: BRL.");

        var monthly = await _aggregator.GetMonthlyTotalsAsync(userId, 2, today);
        var previous = monthly[0];
        var current = monthly[1];

        sb.AppendLine(
            $"Month to date ({current.Month}): expenses {MoneyFormatter.Format(current.Expenses)}, income {MoneyFormatter.Format(current.Income)}. " +
            $"Previous month ({previous.Month}): expenses {MoneyFormatter.Format(previous.Expenses)}, income {MoneyFormatter.Format(previous.Income)}.");

        var thisMonth = await _aggregator.GetCategoryTotalsAsync(userId, monthStart, today);

        if (thisMonth.Count > 0)
        {
            sb.AppendLine("Top expense categories this month: " + string.Join(
                "; ",
                thisMonth.Take(TopCategories).Select(c => $"{c.Category} {MoneyFormatter.Format(c.Total)}")) + ".");
        }

        var history = await _aggregator.GetCategoryTotalsAsync(
            userId,
            monthStart.AddMonths(-HistoryMonths),
            monthStart.AddDays(-1));

        if (history.Count > 0)
        {
            sb.AppendLine($"Average monthly expense per category over the previous {HistoryMonths} months (top {TopCategories}): " + string.Join(
                "; ",
                history.Take(TopCategories).Select(c => $"{c.Category} {MoneyFormatter.Format(c.Total / HistoryMonths)}")) + ".");
        }

        await AppendConfigurationAsync(sb, userId);
        await AppendCardsAsync(sb, userId);
        await AppendGoalsAsync(sb, userId);

        sb.AppendLine(await _aggregator.HasCardInvoiceCategoryAsync(userId)
            ? "Card invoice payment category: configured."
            : "Card invoice payment category: NOT configured (card bills typed manually as ordinary transactions may be counted twice).");

        return sb.ToString();
    }

    private async Task AppendConfigurationAsync(StringBuilder sb, int userId)
    {
        var categories = (await _categoryRepository.GetAllAsync(userId)).ToList();
        var items = (await _categoryItemRepository.GetAllByUserIdAsync(userId)).ToList();
        var methods = (await _paymentMethodRepository.GetAllAsync(userId)).ToList();

        foreach (var (code, label) in CategoryTypeLabels)
        {
            var ofType = categories.Where(c => c.CodTypeTransaction == code).Take(MaxCategoriesPerType).ToList();

            if (ofType.Count == 0)
            {
                sb.AppendLine($"{label} categories: none.");
                continue;
            }

            var described = ofType.Select(c =>
            {
                var subcategories = items.Where(i => i.CategoryId == c.Id).Take(MaxSubcategoriesPerCategory).Select(i => i.Description).ToList();

                return subcategories.Count == 0
                    ? c.Description
                    : $"{c.Description} (subcategories: {string.Join(", ", subcategories)})";
            });

            sb.AppendLine($"{label} categories: {string.Join("; ", described)}.");
        }

        sb.AppendLine(methods.Count == 0
            ? "Payment methods: none."
            : "Payment methods: " + string.Join(", ", methods.Select(m => m.Description)) + ".");
    }

    private async Task AppendCardsAsync(StringBuilder sb, int userId)
    {
        var cards = (await _creditCardRepository.GetAllAsync(userId)).ToList();

        if (cards.Count == 0)
        {
            sb.AppendLine("Credit cards: none.");
            return;
        }

        var parts = new List<string>();

        foreach (var card in cards)
        {
            var used = await _purchaseRepository.SumPendingInstallmentsByCardAsync(card.Id, userId);

            var pending = await _invoiceRepository.GetPendingAmountsByCardAsync(card.Id, userId);

            var next = (await _invoiceRepository.GetByCardAsync(card.Id, userId))
                .Where(i => !i.IsPaid && pending.ContainsKey(i.Id))
                .OrderBy(i => i.ReferenceYear)
                .ThenBy(i => i.ReferenceMonth)
                .FirstOrDefault();

            var invoice = next is null
                ? "no unpaid invoice"
                : $"next unpaid invoice {next.ReferenceYear}-{next.ReferenceMonth:00} due {next.DueDate:yyyy-MM-dd} {MoneyFormatter.Format(pending[next.Id])}";

            parts.Add($"{card.Description} (limit {MoneyFormatter.Format(card.CreditLimit)}, used {MoneyFormatter.Format(used)}, {invoice})");
        }

        sb.AppendLine("Credit cards: " + string.Join("; ", parts) + ".");
    }

    private async Task AppendGoalsAsync(StringBuilder sb, int userId)
    {
        var goals = (await _goalRepository.GetActiveAsync(userId)).Count();
        var commitments = (await _commitmentRepository.GetByUserAsync(userId)).Count(c => c.Active);

        sb.AppendLine($"Active goals: {goals}. Active planned recurring commitments: {commitments}.");
    }
}
