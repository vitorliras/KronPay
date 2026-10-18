using System.Globalization;
using Application.Abstractions.AiChat;
using Application.DTOs.AiChat;
using Application.DTOs.Card.CardPurchases;
using Application.DTOs.Transactions;
using Application.Abstractions.Common;
using Application.Abstractions.AI;
using Application.Services.Assistant;
using Application.Services.Card;
using Application.Services.Spending;
using Application.Services.Transactions;
using Domain.Entities.Card;
using Domain.Entities.Configuration;
using Domain.Enums.Goals;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Domain.Interfaces.Goals;
using Domain.Interfaces.Planning;
using Domain.Interfaces.Transactions;
using Domain.Services.Assistant;
using Domain.Services.Transactions;
using Shared.AI;

namespace Application.Services.AiChat;

public sealed class ChatToolbox : IChatToolbox
{
    private const int MaxRangeDays = 366;
    private const int MaxCategories = 15;
    private const int MaxSearchRows = 30;
    private const int MaxTextLength = 4000;
    private const int MaxCommitmentDescriptionLength = 60;
    private const int ReviewMonths = 6;
    private const int MaxUncategorizedDescriptions = 10;

    private static readonly IReadOnlyDictionary<string, (string Path, string LabelKey)> ExtraScreens =
        new Dictionary<string, (string Path, string LabelKey)>
        {
            ["invoices"] = (InvoicesPath, "Invoices"),
            ["goals-history"] = ("/goals-history", "GoalsHistory"),
            ["achievements"] = ("/profile/achievements", "Achievements")
        };
    private const string InvoicesPath = "/credit-card/invoices";

    private readonly SpendingAggregator _aggregator;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ICardInvoiceRepository _invoiceRepository;
    private readonly ICardPurchaseRepository _purchaseRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IFinancialGoalRepository _goalRepository;
    private readonly IPlannedCommitmentRepository _commitmentRepository;
    private readonly IParameterFuzzyMatcher _fuzzyMatcher;
    private readonly AssistantTree _assistantTree;
    private readonly ChatActionCollector _actions;
    private readonly ScreenFilterBuilder _screenFilterBuilder = new();
    private readonly TransactionDraftCategorizer _categorizer;
    private readonly ChatAttachmentToolService _attachments;
    private readonly CardPurchasePreviewBuilder _previewBuilder;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICategoryItemRepository _categoryItemRepository;
    private readonly ICurrentUserService _currentUser;

    public ChatToolbox(
        SpendingAggregator aggregator,
        ITransactionRepository transactionRepository,
        ICreditCardRepository creditCardRepository,
        ICardInvoiceRepository invoiceRepository,
        ICardPurchaseRepository purchaseRepository,
        IPaymentMethodRepository paymentMethodRepository,
        IFinancialGoalRepository goalRepository,
        IPlannedCommitmentRepository commitmentRepository,
        IParameterFuzzyMatcher fuzzyMatcher,
        AssistantTree assistantTree,
        ChatActionCollector actions,
        TransactionDraftCategorizer categorizer,
        ChatAttachmentToolService attachments,
        CardPurchasePreviewBuilder previewBuilder,
        ICategoryRepository categoryRepository,
        ICategoryItemRepository categoryItemRepository,
        ICurrentUserService currentUser)
    {
        _aggregator = aggregator;
        _transactionRepository = transactionRepository;
        _creditCardRepository = creditCardRepository;
        _invoiceRepository = invoiceRepository;
        _purchaseRepository = purchaseRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _goalRepository = goalRepository;
        _commitmentRepository = commitmentRepository;
        _fuzzyMatcher = fuzzyMatcher;
        _assistantTree = assistantTree;
        _actions = actions;
        _categorizer = categorizer;
        _attachments = attachments;
        _previewBuilder = previewBuilder;
        _categoryRepository = categoryRepository;
        _categoryItemRepository = categoryItemRepository;
        _currentUser = currentUser;
    }

    public async Task<ChatToolResult> GetSpendingByCategory(string startDate, string endDate)
    {
        if (!TryDate(startDate, out var start) || !TryDate(endDate, out var end))
            return ChatToolResult.Failure("Invalid date. Use yyyy-MM-dd.");

        if (end < start || (end - start).TotalDays > MaxRangeDays)
            return ChatToolResult.Failure("Invalid range. The end must not be before the start and the range is limited to one year.");

        var userId = _currentUser.UserId;
        var totals = await _aggregator.GetCategoryTotalsAsync(userId, start, end);

        var note = "Credit card purchases are counted in the month of their invoice and card invoice payments are excluded.";

        if (!await _aggregator.HasCardInvoiceCategoryAsync(userId))
            note += " WARNING: no category is marked as card invoice payment, so card bills typed manually as ordinary transactions may be counted twice.";

        return ChatToolResult.Success(new
        {
            startDate = Iso(start),
            endDate = Iso(end),
            total = Round(totals.Sum(t => t.Total)),
            categories = totals.Take(MaxCategories).Select(t => new
            {
                category = t.Category,
                total = Round(t.Total),
                count = t.Count,
                cardPortion = Round(t.CardPortion)
            }),
            note
        });
    }

    public async Task<ChatToolResult> GetMonthlyTotals(int months)
    {
        var count = Math.Clamp(months, 1, 12);
        var totals = await _aggregator.GetMonthlyTotalsAsync(_currentUser.UserId, count, BrazilClock.Today());

        return ChatToolResult.Success(totals.Select(t => new
        {
            month = t.Month,
            expenses = Round(t.Expenses),
            income = Round(t.Income),
            balance = Round(t.Balance)
        }));
    }

    public async Task<ChatToolResult> GetLifetimeTotals()
    {
        var today = BrazilClock.Today();
        var totals = await _aggregator.GetLifetimeTotalsAsync(_currentUser.UserId, today);

        if (totals is null)
            return ChatToolResult.Failure("The user has no transactions yet.");

        return ChatToolResult.Success(new
        {
            firstDate = Iso(totals.FirstDate),
            endDate = Iso(today),
            expenses = Round(totals.Expenses),
            income = Round(totals.Income),
            balance = Round(totals.Balance),
            years = totals.Years.Select(y => new
            {
                year = y.Year,
                expenses = Round(y.Expenses),
                income = Round(y.Income),
                balance = Round(y.Balance)
            }),
            note = "Credit card purchases are counted in the month of their invoice and card invoice payments are excluded."
        });
    }

    public async Task<ChatToolResult> SearchTransactions(
        string? text,
        string? category,
        string? startDate,
        string? endDate,
        string? kind,
        int limit)
    {
        var today = BrazilClock.Today();
        var start = today.AddDays(-90);
        var end = today;

        if (!string.IsNullOrWhiteSpace(startDate) && !TryDate(startDate, out start))
            return ChatToolResult.Failure("Invalid startDate. Use yyyy-MM-dd.");

        if (!string.IsNullOrWhiteSpace(endDate) && !TryDate(endDate, out end))
            return ChatToolResult.Failure("Invalid endDate. Use yyyy-MM-dd.");

        if (end < start || (end - start).TotalDays > MaxRangeDays)
            return ChatToolResult.Failure("Invalid range. The range is limited to one year.");

        var userId = _currentUser.UserId;
        var names = await _aggregator.CategoryNamesAsync(userId);
        var wanted = (kind ?? "expense").Trim().ToLowerInvariant();

        var rows = new List<SearchRow>();

        if (wanted is "expense" or "all")
        {
            var entries = await _aggregator.GetExpenseEntriesAsync(userId, start, end);
            rows.AddRange(entries.Select(e => new SearchRow(e.Date, e.Description, e.Amount, "expense", e.CategoryId, e.FromCard)));
        }

        if (wanted is "income" or "all")
        {
            var incomes = (await _transactionRepository.GetByPeriodAsync(userId, start, end.AddDays(1).AddTicks(-1)))
                .Where(t => t.CodTypeTransaction == "I" && t.Status != "C");

            rows.AddRange(incomes.Select(t => new SearchRow(t.TransactionDate, t.Description, t.Amount, "income", t.CategoryId, false)));
        }

        var normalizedText = TransactionDescriptionNormalizer.Normalize(text);
        var normalizedCategory = TransactionDescriptionNormalizer.Normalize(category);

        var filtered = rows
            .Where(r => normalizedText.Length == 0
                        || TransactionDescriptionNormalizer.Normalize(r.Description).Contains(normalizedText))
            .Where(r => normalizedCategory.Length == 0
                        || (r.CategoryId is { } id
                            && names.TryGetValue(id, out var name)
                            && TransactionDescriptionNormalizer.Normalize(name).Contains(normalizedCategory)))
            .OrderByDescending(r => r.Date)
            .ToList();

        return ChatToolResult.Success(new
        {
            totalMatches = filtered.Count,
            totalAmount = Round(filtered.Sum(r => r.Amount)),
            rows = filtered.Take(Math.Clamp(limit, 1, MaxSearchRows)).Select(r => new
            {
                date = Iso(r.Date),
                description = r.Description,
                amount = Round(r.Amount),
                type = r.Kind,
                category = r.CategoryId is { } id && names.TryGetValue(id, out var name) ? name : null,
                source = r.FromCard ? "card" : "account"
            }),
            note = "Card rows are dated at the first day of their invoice month."
        });
    }

    public async Task<ChatToolResult> GetCardsOverview()
    {
        var userId = _currentUser.UserId;
        var cards = (await _creditCardRepository.GetAllAsync(userId)).ToList();

        var result = new List<object>();

        foreach (var card in cards)
        {
            var used = await _purchaseRepository.SumPendingInstallmentsByCardAsync(card.Id, userId);

            var pending = await _invoiceRepository.GetPendingAmountsByCardAsync(card.Id, userId);

            var unpaid = (await _invoiceRepository.GetByCardAsync(card.Id, userId))
                .Where(i => !i.IsPaid && pending.ContainsKey(i.Id))
                .OrderBy(i => i.ReferenceYear)
                .ThenBy(i => i.ReferenceMonth)
                .Take(3)
                .Select(i => new
                {
                    referenceMonth = $"{i.ReferenceYear}-{i.ReferenceMonth:00}",
                    dueDate = Iso(i.DueDate),
                    amount = Round(pending[i.Id]),
                    status = i.Status == "F" ? "closed" : "open"
                })
                .ToList();

            result.Add(new
            {
                card = card.Description,
                limit = Round(card.CreditLimit),
                used = Round(used),
                available = Round(card.CreditLimit - used),
                unpaidInvoices = unpaid
            });
        }

        return ChatToolResult.Success(result);
    }

    public async Task<ChatToolResult> GetGoalsAndCommitments()
    {
        var userId = _currentUser.UserId;

        var goals = (await _goalRepository.GetActiveAsync(userId))
            .Select(g => new
            {
                goal = g.Description,
                target = Round(g.TargetAmount),
                current = Round(g.CurrentAmount),
                percent = g.TargetAmount > 0 ? Math.Round(g.CurrentAmount / g.TargetAmount * 100m, 1) : 0m,
                targetDate = Iso(g.TargetDate)
            });

        var commitments = (await _commitmentRepository.GetByUserAsync(userId))
            .Where(c => c.Active)
            .Select(c => new
            {
                description = c.Description,
                amount = Round(c.Amount),
                direction = c.Direction == "I" ? "income" : "expense",
                periodicity = c.Periodicity switch { "M" => "monthly", "S" => "weekly", "A" => "yearly", _ => "once" },
                startDate = Iso(c.StartDate),
                endDate = c.EndDate is { } e ? Iso(e) : null
            });

        return ChatToolResult.Success(new { goals, commitments });
    }

    public async Task<ChatToolResult> GetRecurringExpenses()
    {
        var userId = _currentUser.UserId;
        var today = BrazilClock.Today();
        var start = new DateTime(today.Year, today.Month, 1).AddMonths(-6);

        var entries = await _aggregator.GetExpenseEntriesAsync(userId, start, today);
        var names = await _aggregator.CategoryNamesAsync(userId);

        return ChatToolResult.Success(RecurringExpenseDetector.Detect(entries).Take(20).Select(r => new
        {
            description = r.Description,
            monthsSeen = r.MonthsCount,
            averageMonthlyAmount = Round(r.AverageAmount),
            lastSeenMonth = r.LastDate.ToString("yyyy-MM", CultureInfo.InvariantCulture),
            category = r.CategoryId is { } id && names.TryGetValue(id, out var name) ? name : null
        }));
    }

    public async Task<ChatToolResult> ReviewCategories()
    {
        var userId = _currentUser.UserId;
        var today = BrazilClock.Today();
        var start = new DateTime(today.Year, today.Month, 1).AddMonths(-(ReviewMonths - 1));

        var categories = (await _categoryRepository.GetAllAsync(userId)).ToList();
        var items = (await _categoryItemRepository.GetAllByUserIdAsync(userId)).ToList();
        var entries = await _aggregator.GetExpenseEntriesAsync(userId, start, today);

        var incomes = (await _transactionRepository.GetByPeriodAsync(userId, start, today.AddDays(1).AddTicks(-1)))
            .Where(t => t.CodTypeTransaction == "I" && t.Status != "C")
            .ToList();

        var usage = entries
            .Select(e => (e.CategoryId, e.CategoryItemId))
            .Concat(incomes.Select(t => (t.CategoryId, t.CategoryItemId)))
            .ToList();

        var reviewed = categories.Select(c => new
        {
            category = c.Description,
            type = c.CodTypeTransaction switch { "E" => "expense", "I" => "income", _ => "investment" },
            timesUsed = usage.Count(u => u.CategoryId == c.Id),
            subcategories = items
                .Where(i => i.CategoryId == c.Id)
                .Select(i => new { name = i.Description, timesUsed = usage.Count(u => u.CategoryItemId == i.Id) })
                .ToList()
        }).ToList();

        var uncategorized = entries.Where(e => e.CategoryId is null).ToList();

        return ChatToolResult.Success(new
        {
            periodMonths = ReviewMonths,
            categories = reviewed,
            unusedCategories = reviewed.Where(c => c.timesUsed == 0).Select(c => c.category),
            uncategorizedExpenses = new
            {
                count = uncategorized.Count,
                total = Round(uncategorized.Sum(e => e.Amount)),
                topDescriptions = uncategorized
                    .GroupBy(e => TransactionDescriptionNormalizer.Normalize(e.Description))
                    .Select(g => new { description = g.First().Description, times = g.Count(), total = Round(g.Sum(e => e.Amount)) })
                    .OrderByDescending(x => x.total)
                    .Take(MaxUncategorizedDescriptions)
            }
        });
    }

    public async Task<ChatToolResult> OpenScreen(
        string screen,
        bool goNow,
        string? tab = null,
        string? startDate = null,
        string? endDate = null,
        int? month = null,
        int? year = null,
        string? type = null,
        string? categoryName = null,
        string? search = null,
        int? horizonMonths = null,
        string? cardName = null,
        string? goalName = null)
    {
        var id = (screen ?? string.Empty).Trim().ToLowerInvariant();

        if (!TryScreen(id, out var path, out var labelKey))
            return ChatToolResult.Failure($"Unknown screen. Valid screens: {string.Join(", ", ScreenIds())}.");

        var userId = _currentUser.UserId;
        var ignored = new List<string>();

        DateTime? start = null;
        DateTime? end = null;

        if (!string.IsNullOrWhiteSpace(startDate))
        {
            if (TryDate(startDate, out var parsedStart)) start = parsedStart; else ignored.Add($"start date '{startDate}'");
        }

        if (!string.IsNullOrWhiteSpace(endDate))
        {
            if (TryDate(endDate, out var parsedEnd)) end = parsedEnd; else ignored.Add($"end date '{endDate}'");
        }

        var filterType = string.IsNullOrWhiteSpace(type) ? null : TransactionType(type);

        if (!string.IsNullOrWhiteSpace(type) && filterType is null)
            ignored.Add($"type '{type}'");

        int? categoryId = null;

        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            var categories = (await _categoryRepository.GetAllAsync(userId)).ToList();
            var resolution = NameResolver.Resolve(categoryName, categories, c => c.Id, c => c.Description, _fuzzyMatcher);

            if (resolution.Match is null)
            {
                return resolution.IsAmbiguous
                    ? ChatToolResult.Failure($"Ambiguous category. Ask the user which one: {string.Join(", ", resolution.Candidates.Select(c => c.Description))}.")
                    : ChatToolResult.Failure($"Category not found. The user's categories are: {string.Join(", ", categories.Select(c => c.Description))}.");
            }

            categoryId = resolution.Match.Id;
        }

        int? cardId = null;

        if (!string.IsNullOrWhiteSpace(cardName))
        {
            var cards = (await _creditCardRepository.GetAllAsync(userId)).ToList();
            var resolution = ResolveCard(cardName, cards);

            if (resolution.Match is null)
                return CardNotResolved(resolution, cards);

            cardId = resolution.Match.Id;
        }

        int? goalId = null;

        if (!string.IsNullOrWhiteSpace(goalName))
        {
            var goals = (await _goalRepository.GetActiveAsync(userId)).ToList();
            var resolution = NameResolver.Resolve(goalName, goals, g => g.Id, g => g.Description, _fuzzyMatcher);

            if (resolution.Match is null)
            {
                return resolution.IsAmbiguous
                    ? ChatToolResult.Failure($"Ambiguous goal. Ask the user which one: {string.Join(", ", resolution.Candidates.Select(g => g.Description))}.")
                    : ChatToolResult.Failure($"Goal not found. The user's active goals are: {string.Join(", ", goals.Select(g => g.Description))}.");
            }

            goalId = resolution.Match.Id;
        }

        var filters = _screenFilterBuilder.Build(
            id,
            new ScreenFilterInput(tab, start, end, month, year, filterType, categoryId, search, horizonMonths, cardId, goalId),
            BrazilClock.Today());

        ignored.AddRange(filters.Ignored);

        _actions.Add(new AiChatActionResponse(
            goNow ? AiChatActionTypes.NavigateNow : AiChatActionTypes.Navigate,
            labelKey,
            path,
            filters.QueryParams.Count == 0 ? null : filters.QueryParams));

        var applied = filters.Applied.Count == 0 ? "no filters" : string.Join(", ", filters.Applied);
        var ignoredText = ignored.Count == 0 ? string.Empty : $" These were NOT applied, tell the user: {string.Join("; ", ignored)}.";

        return ChatToolResult.Success(
            message: goNow
                ? $"The user is being taken to the screen right now. Applied: {applied}. Tell the user in one short sentence what was applied (period, tab, etc.).{ignoredText}"
                : $"A button to open the screen will be shown to the user.{ignoredText}");
    }

    public Task<ChatToolResult> StartTour(string screen)
    {
        var id = (screen ?? string.Empty).Trim().ToLowerInvariant();

        if (id == "invoices" || !TryScreen(id, out var path, out var labelKey))
            return Task.FromResult(ChatToolResult.Failure("Unknown screen for a tour. Valid screens: dashboard, transactions, reports, planning, cards, goals, config."));

        _actions.Add(new AiChatActionResponse(
            AiChatActionTypes.StartTour,
            labelKey,
            path,
            new Dictionary<string, string> { ["tour"] = "true" }));

        return Task.FromResult(ChatToolResult.Success(message: "A button to start the tour will be shown to the user."));
    }

    public async Task<ChatToolResult> PrepareTransactions(IReadOnlyList<TransactionToolItem>? items = null)
    {
        if (items is null || items.Count == 0)
            return Missing("the transactions (description and amount of each one)");

        var userId = _currentUser.UserId;
        var today = BrazilClock.Today();
        var categories = (await _categoryRepository.GetAllAsync(userId)).ToList();
        var methods = (await _paymentMethodRepository.GetAllAsync(userId)).ToList();

        var missing = new List<string>();
        var drafts = new List<TransactionDraftResponse>();

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var label = items.Count > 1 ? $"item {index + 1}: " : string.Empty;
            var problems = new List<string>();

            if (string.IsNullOrWhiteSpace(item.Description))
                problems.Add("description");

            if (item.Amount is null or <= 0)
                problems.Add("amount");

            var date = today;

            if (!string.IsNullOrWhiteSpace(item.Date) && !TryDate(item.Date, out date))
                problems.Add("date (yyyy-MM-dd)");

            var type = TransactionType(item.Type);

            if (type is null)
                problems.Add("type (expense or income)");

            if (problems.Count > 0)
            {
                missing.Add(label + string.Join(" and ", problems));
                continue;
            }

            var method = NameResolver.Resolve(item.PaymentMethodName, methods, m => m.Id, m => m.Description, _fuzzyMatcher).Match;

            var category = string.IsNullOrWhiteSpace(item.CategoryName)
                ? null
                : NameResolver.Resolve(
                    item.CategoryName,
                    categories.Where(c => c.CodTypeTransaction == type).ToList(),
                    c => c.Id,
                    c => c.Description,
                    _fuzzyMatcher).Match;

            drafts.Add(new TransactionDraftResponse(
                item.Description!.Trim(),
                Math.Round(item.Amount!.Value, 2),
                Iso(date),
                type!,
                method?.Id,
                category?.Id,
                null,
                null));
        }

        if (missing.Count > 0)
            return Missing(string.Join("; ", missing));

        drafts = (await _categorizer.SuggestAsync(userId, drafts, categories)).ToList();

        _actions.Add(new AiChatActionResponse(
            AiChatActionTypes.OpenTransactionsModal,
            Path: "/transaction",
            TransactionDrafts: drafts));

        return Opened(new { prepared = drafts.Count });
    }

    public async Task<ChatToolResult> PrepareCardPurchases(
        string? cardName = null,
        IReadOnlyList<CardPurchaseToolItem>? items = null)
    {
        var userId = _currentUser.UserId;
        var cards = (await _creditCardRepository.GetAllAsync(userId)).ToList();

        if (string.IsNullOrWhiteSpace(cardName))
        {
            return cards.Count == 0
                ? ChatToolResult.Failure("The user has no credit cards registered.")
                : Missing($"the credit card (the user has: {string.Join(", ", cards.Select(c => c.Description))})");
        }

        var resolution = ResolveCard(cardName, cards);

        if (resolution.Match is null)
            return CardNotResolved(resolution, cards);

        if (items is null || items.Count == 0)
            return Missing("the purchases (description and total amount of each one)");

        var today = BrazilClock.Today();
        var categories = (await _categoryRepository.GetAllAsync(userId))
            .Where(c => c.CodTypeTransaction == "E")
            .ToList();

        var missing = new List<string>();
        var extracted = new List<ExtractedCardPurchase>();
        var requestedCategories = new List<int?>();

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var label = items.Count > 1 ? $"item {index + 1}: " : string.Empty;
            var problems = new List<string>();

            if (string.IsNullOrWhiteSpace(item.Description))
                problems.Add("description");

            if (item.TotalAmount is null or <= 0)
                problems.Add("total amount");

            var date = today;

            if (!string.IsNullOrWhiteSpace(item.Date) && !TryDate(item.Date, out date))
                problems.Add("date (yyyy-MM-dd)");

            if (problems.Count > 0)
            {
                missing.Add(label + string.Join(" and ", problems));
                continue;
            }

            var description = item.Description!.Trim();

            extracted.Add(new ExtractedCardPurchase(
                date,
                description.Length <= ExtractedCardPurchaseValidator.MaxDescriptionLength
                    ? description
                    : description[..ExtractedCardPurchaseValidator.MaxDescriptionLength].TrimEnd(),
                Math.Round(item.TotalAmount!.Value, 2),
                (short)Math.Clamp(item.Installments ?? 1, 1, ExtractedCardPurchaseValidator.MaxInstallments)));

            requestedCategories.Add(string.IsNullOrWhiteSpace(item.CategoryName)
                ? null
                : NameResolver.Resolve(item.CategoryName, categories, c => c.Id, c => c.Description, _fuzzyMatcher).Match?.Id);
        }

        if (missing.Count > 0)
            return Missing(string.Join("; ", missing));

        var preview = await _previewBuilder.BuildAsync(userId, resolution.Match.Id, extracted, skipped: 0, useAi: false);

        var purchases = preview.Purchases
            .Select((p, i) => requestedCategories[i] is { } id
                ? p with { CategoryId = id, CategoryItemId = null, ClassificationSource = null }
                : p)
            .ToList();

        _actions.Add(new AiChatActionResponse(
            AiChatActionTypes.OpenCardPurchasesModal,
            Path: InvoicesPath,
            QueryParams: new Dictionary<string, string> { ["cardId"] = resolution.Match.Id.ToString(CultureInfo.InvariantCulture) },
            CreditCardId: resolution.Match.Id,
            CardPurchasesPreview: preview with { Purchases = purchases }));

        return Opened(new { card = resolution.Match.Description, prepared = purchases.Count });
    }

    public async Task<ChatToolResult> PrepareCategory(string? description = null, string? type = null)
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(description))
            missing.Add("category name");

        var code = CategoryType(type);

        if (code is null)
            missing.Add("type (expense, income or investment)");

        if (missing.Count > 0)
            return Missing(string.Join(" and ", missing));

        var existing = (await _categoryRepository.GetAllAsync(_currentUser.UserId))
            .Any(c => TransactionDescriptionNormalizer.Normalize(c.Description) == TransactionDescriptionNormalizer.Normalize(description));

        if (existing)
            return ChatToolResult.Failure("A category with this name already exists. Tell the user.");

        _actions.Add(new AiChatActionResponse(
            AiChatActionTypes.OpenCategoryModal,
            Path: "/config",
            QueryParams: new Dictionary<string, string> { ["tab"] = "categories" },
            CategoryDraft: new CategoryDraftResponse(description!.Trim(), code!)));

        return Opened();
    }

    public async Task<ChatToolResult> PrepareSubcategory(string? categoryName = null, string? description = null)
    {
        var userId = _currentUser.UserId;
        var categories = (await _categoryRepository.GetAllAsync(userId)).ToList();

        if (string.IsNullOrWhiteSpace(categoryName))
            return Missing($"the parent category (the user has: {string.Join(", ", categories.Select(c => c.Description))})");

        var resolution = NameResolver.Resolve(categoryName, categories, c => c.Id, c => c.Description, _fuzzyMatcher);

        if (resolution.Match is null)
        {
            return resolution.IsAmbiguous
                ? ChatToolResult.Failure($"Ambiguous category. Ask the user which one: {string.Join(", ", resolution.Candidates.Select(c => c.Description))}.")
                : ChatToolResult.Failure($"Category not found. The user's categories are: {string.Join(", ", categories.Select(c => c.Description))}.");
        }

        if (string.IsNullOrWhiteSpace(description))
            return Missing("subcategory name");

        var existing = (await _categoryItemRepository.GetAllAsync(resolution.Match.Id))
            .Any(i => TransactionDescriptionNormalizer.Normalize(i.Description) == TransactionDescriptionNormalizer.Normalize(description));

        if (existing)
            return ChatToolResult.Failure("A subcategory with this name already exists in that category. Tell the user.");

        _actions.Add(new AiChatActionResponse(
            AiChatActionTypes.OpenSubcategoryModal,
            Path: "/config",
            QueryParams: new Dictionary<string, string> { ["tab"] = "subcategories" },
            SubcategoryDraft: new SubcategoryDraftResponse(resolution.Match.Id, description!.Trim())));

        return Opened(new { category = resolution.Match.Description });
    }

    public async Task<ChatToolResult> PreparePaymentMethod(string? description = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Missing("payment method name");

        var existing = (await _paymentMethodRepository.GetAllAsync(_currentUser.UserId))
            .Any(m => TransactionDescriptionNormalizer.Normalize(m.Description) == TransactionDescriptionNormalizer.Normalize(description));

        if (existing)
            return ChatToolResult.Failure("A payment method with this name already exists. Tell the user.");

        _actions.Add(new AiChatActionResponse(
            AiChatActionTypes.OpenPaymentMethodModal,
            Path: "/config",
            QueryParams: new Dictionary<string, string> { ["tab"] = "payments" },
            PaymentMethodDraft: new PaymentMethodDraftResponse(description.Trim())));

        return Opened();
    }

    public Task<ChatToolResult> PrepareFinancialGoal(
        string? description = null,
        decimal? targetAmount = null,
        string? targetDate = null,
        string? priority = null)
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(description))
            missing.Add("what the goal is for");

        if (targetAmount is null or <= 0)
            missing.Add("target amount");

        var date = default(DateTime);

        if (string.IsNullOrWhiteSpace(targetDate) || !TryDate(targetDate, out date))
            missing.Add("target date (yyyy-MM-dd)");
        else if (date <= BrazilClock.Today())
            missing.Add("target date in the future");

        if (missing.Count > 0)
            return Task.FromResult(Missing(string.Join(" and ", missing)));

        _actions.Add(new AiChatActionResponse(
            AiChatActionTypes.OpenGoalModal,
            Path: "/goals",
            FinancialGoalDraft: new FinancialGoalDraftResponse(
                description!.Trim(),
                Math.Round(targetAmount!.Value, 2),
                Iso(date),
                PriorityOf(priority))));

        return Task.FromResult(Opened());
    }

    public async Task<ChatToolResult> PrepareCategoryBudgetGoal(
        string? categoryName = null,
        decimal? monthlyLimit = null,
        string? priority = null)
    {
        var categories = (await _categoryRepository.GetAllAsync(_currentUser.UserId))
            .Where(c => c.CodTypeTransaction == "E")
            .ToList();

        if (string.IsNullOrWhiteSpace(categoryName))
            return Missing($"the expense category (the user has: {string.Join(", ", categories.Select(c => c.Description))})");

        var resolution = NameResolver.Resolve(categoryName, categories, c => c.Id, c => c.Description, _fuzzyMatcher);

        if (resolution.Match is null)
        {
            return resolution.IsAmbiguous
                ? ChatToolResult.Failure($"Ambiguous category. Ask the user which one: {string.Join(", ", resolution.Candidates.Select(c => c.Description))}.")
                : ChatToolResult.Failure($"Expense category not found. The user's expense categories are: {string.Join(", ", categories.Select(c => c.Description))}.");
        }

        if (monthlyLimit is null or <= 0)
            return Missing("monthly limit");

        _actions.Add(new AiChatActionResponse(
            AiChatActionTypes.OpenCategoryGoalModal,
            Path: "/goals",
            CategoryBudgetGoalDraft: new CategoryBudgetGoalDraftResponse(
                resolution.Match.Id,
                Math.Round(monthlyLimit.Value, 2),
                PriorityOf(priority))));

        return Opened(new { category = resolution.Match.Description });
    }

    public async Task<ChatToolResult> PrepareGoalContribution(string? goalName = null, decimal? amount = null)
    {
        var goals = (await _goalRepository.GetActiveAsync(_currentUser.UserId))
            .Where(g => g.Status == FinancialGoalStatus.Active)
            .ToList();

        if (goals.Count == 0)
            return ChatToolResult.Failure("The user has no active financial goals. Tell the user.");

        if (string.IsNullOrWhiteSpace(goalName))
            return Missing($"the goal (the user has: {string.Join(", ", goals.Select(g => g.Description))})");

        var resolution = NameResolver.Resolve(goalName, goals, g => g.Id, g => g.Description, _fuzzyMatcher);

        if (resolution.Match is null)
        {
            return resolution.IsAmbiguous
                ? ChatToolResult.Failure($"Ambiguous goal. Ask the user which one: {string.Join(", ", resolution.Candidates.Select(g => g.Description))}.")
                : ChatToolResult.Failure($"Goal not found. The user has these active goals: {string.Join(", ", goals.Select(g => g.Description))}.");
        }

        if (amount is null or <= 0)
            return Missing("contribution amount");

        _actions.Add(new AiChatActionResponse(
            AiChatActionTypes.OpenGoalContributionModal,
            Path: "/goals",
            GoalContributionDraft: new GoalContributionDraftResponse(
                resolution.Match.Id,
                resolution.Match.Description,
                Math.Round(amount.Value, 2))));

        return Opened(new { goal = resolution.Match.Description });
    }

    public async Task<ChatToolResult> PreparePlannedCommitment(
        string? description = null,
        decimal? amount = null,
        string? direction = null,
        string? periodicity = null,
        string? startDate = null,
        string? endDate = null,
        string? categoryName = null)
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(description))
            missing.Add("description");

        if (amount is null or <= 0)
            missing.Add("amount");

        var directionCode = CommitmentDirection(direction);

        if (directionCode is null)
            missing.Add("direction (expense or income)");

        var periodicityCode = CommitmentPeriodicity(periodicity);

        if (periodicityCode is null)
            missing.Add("periodicity (once, monthly, weekly or annual)");

        var start = BrazilClock.Today();

        if (!string.IsNullOrWhiteSpace(startDate) && !TryDate(startDate, out start))
            missing.Add("start date (yyyy-MM-dd)");

        DateTime? end = null;

        if (!string.IsNullOrWhiteSpace(endDate))
        {
            if (!TryDate(endDate, out var parsedEnd))
                missing.Add("end date (yyyy-MM-dd)");
            else if (parsedEnd < start)
                missing.Add("end date after the start date");
            else
                end = parsedEnd;
        }

        if (missing.Count > 0)
            return Missing(string.Join(" and ", missing));

        var categoryType = directionCode == "I" ? "I" : "E";

        var category = string.IsNullOrWhiteSpace(categoryName)
            ? null
            : NameResolver.Resolve(
                categoryName,
                (await _categoryRepository.GetAllAsync(_currentUser.UserId)).Where(c => c.CodTypeTransaction == categoryType).ToList(),
                c => c.Id,
                c => c.Description,
                _fuzzyMatcher).Match;

        var trimmed = description!.Trim();

        _actions.Add(new AiChatActionResponse(
            AiChatActionTypes.OpenPlannedCommitmentModal,
            Path: "/planning",
            PlannedCommitmentDraft: new PlannedCommitmentDraftResponse(
                trimmed.Length <= MaxCommitmentDescriptionLength ? trimmed : trimmed[..MaxCommitmentDescriptionLength].TrimEnd(),
                Math.Round(amount!.Value, 2),
                directionCode!,
                periodicityCode!,
                Iso(start),
                end is null ? null : Iso(end.Value),
                category?.Id)));

        return Opened();
    }

    public Task<ChatToolResult> SummarizeAttachment(string? attachmentId = null) =>
        _attachments.SummarizeAsync(attachmentId);

    public Task<ChatToolResult> GetAttachmentTransactions(
        string? attachmentId = null,
        string? fromDate = null,
        string? toDate = null,
        string? text = null,
        string? type = null,
        string? kind = null,
        int limit = 20) =>
        _attachments.GetTransactionsAsync(attachmentId, fromDate, toDate, text, type, kind, limit);

    public Task<ChatToolResult> CheckAttachmentDuplicates(
        string? attachmentId = null,
        string? fromDate = null,
        string? toDate = null) =>
        _attachments.CheckDuplicatesAsync(attachmentId, fromDate, toDate);

    public Task<ChatToolResult> PrepareTransactionsFromAttachment(
        string? attachmentId = null,
        string? fromDate = null,
        string? toDate = null,
        string? type = null,
        string? text = null,
        string? excludeText = null,
        bool includeDuplicates = false,
        bool includeCardInvoicePayments = true) =>
        _attachments.PrepareTransactionsAsync(
            attachmentId, fromDate, toDate, type, text, excludeText, includeDuplicates, includeCardInvoicePayments);

    public async Task<ChatToolResult> PrepareInvoicePayment(
        string cardName,
        int? month,
        int? year,
        string? paymentMethodName)
    {
        var userId = _currentUser.UserId;
        var cards = (await _creditCardRepository.GetAllAsync(userId)).ToList();
        var resolution = ResolveCard(cardName, cards);

        if (resolution.Match is null)
            return CardNotResolved(resolution, cards);

        if (_actions.Actions.Any(a => a.Type == AiChatActionTypes.ConfirmInvoicePayment))
            return ChatToolResult.Failure("Only one invoice payment can be prepared per message. Tell the user you prepared the first one and that they can ask for the next after finishing it.");

        var card = resolution.Match;
        var invoices = (await _invoiceRepository.GetByCardAsync(card.Id, userId)).ToList();
        var pending = await _invoiceRepository.GetPendingAmountsByCardAsync(card.Id, userId);

        var invoice = month is not null && year is not null
            ? invoices.FirstOrDefault(i => i.ReferenceMonth == month && i.ReferenceYear == year)
            : invoices
                .Where(i => !i.IsPaid && pending.ContainsKey(i.Id))
                .OrderBy(i => i.ReferenceYear)
                .ThenBy(i => i.ReferenceMonth)
                .FirstOrDefault();

        if (invoice is null)
            return ChatToolResult.Failure($"No matching unpaid invoice with a pending amount found for {card.Description}.");

        if (invoice.IsPaid)
            return ChatToolResult.Failure($"The invoice {invoice.ReferenceMonth:00}/{invoice.ReferenceYear} of {card.Description} is already paid.");

        var amount = pending.TryGetValue(invoice.Id, out var pendingAmount) ? pendingAmount : 0m;

        if (amount <= 0)
            return ChatToolResult.Failure($"The invoice {invoice.ReferenceMonth:00}/{invoice.ReferenceYear} of {card.Description} has nothing pending.");

        var methods = (await _paymentMethodRepository.GetAllAsync(userId)).ToList();

        if (methods.Count == 0)
            return ChatToolResult.Failure("The user has no payment methods registered.");

        var options = methods;

        if (!string.IsNullOrWhiteSpace(paymentMethodName))
        {
            var method = NameResolver.Resolve(paymentMethodName, methods, m => m.Id, m => m.Description, _fuzzyMatcher);

            if (method.Match is not null)
                options = new List<Domain.Entities.Configuration.PaymentMethod> { method.Match };
        }

        _actions.Add(new AiChatActionResponse(
            AiChatActionTypes.ConfirmInvoicePayment,
            InvoicePayment: new InvoicePaymentProposalResponse(
                invoice.Id,
                card.Id,
                card.Description,
                invoice.ReferenceYear,
                invoice.ReferenceMonth,
                amount,
                invoice.DueDate,
                options.Select(m => new PaymentMethodOptionResponse(m.Id, m.Description)).ToList())));

        return ChatToolResult.Success(
            new
            {
                card = card.Description,
                invoice = $"{invoice.ReferenceYear}-{invoice.ReferenceMonth:00}",
                amount = Round(amount),
                dueDate = Iso(invoice.DueDate)
            },
            "The chat shows the invoice and the payment method options. The payment is NOT done yet: it is only registered when the user answers. Do not say it was paid.");
    }

    private static ChatToolResult Missing(string what) =>
        ChatToolResult.Failure($"Missing required information: {what}. Nothing was opened. Ask the user ONE short question to get it (offer short answer suggestions) and call this tool again once you have it. Do not guess.");

    private static ChatToolResult Opened(object? data = null) =>
        ChatToolResult.Success(data, "The form was opened on the user's screen to review and save. NOTHING was saved yet. Do not say it was saved.");

    private static string? TransactionType(string? type) =>
        (type ?? "expense").Trim().ToLowerInvariant() switch
        {
            "expense" or "e" => "E",
            "income" or "i" => "I",
            "investment" or "v" => "V",
            "redemption" or "r" => "R",
            _ => null
        };

    private static string? CategoryType(string? type) =>
        (type ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "expense" or "e" => "E",
            "income" or "i" => "I",
            "investment" or "v" => "V",
            _ => null
        };

    private static string? CommitmentDirection(string? direction) =>
        (direction ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "expense" or "o" => "O",
            "income" or "i" => "I",
            _ => null
        };

    private static string? CommitmentPeriodicity(string? periodicity) =>
        (periodicity ?? "monthly").Trim().ToLowerInvariant() switch
        {
            "once" or "u" => "U",
            "monthly" or "m" => "M",
            "weekly" or "s" => "S",
            "annual" or "a" => "A",
            _ => null
        };

    private static int PriorityOf(string? priority) =>
        (priority ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "low" => 0,
            "high" => 2,
            _ => 1
        };

    private NameResolution<CreditCard> ResolveCard(string cardName, IReadOnlyList<CreditCard> cards) =>
        NameResolver.Resolve(cardName, cards, c => c.Id, c => c.Description, _fuzzyMatcher);

    private static ChatToolResult CardNotResolved(NameResolution<CreditCard> resolution, IReadOnlyList<CreditCard> cards)
    {
        if (resolution.IsAmbiguous)
            return ChatToolResult.Failure($"Ambiguous card. Ask the user which one: {string.Join(", ", resolution.Candidates.Select(c => c.Description))}.");

        return ChatToolResult.Failure(cards.Count == 0
            ? "The user has no credit cards registered."
            : $"Card not found. The user's cards are: {string.Join(", ", cards.Select(c => c.Description))}.");
    }

    private IEnumerable<string> ScreenIds() =>
        _assistantTree.GuideScreens.Select(s => s.Id.Replace("guide-", string.Empty)).Append("invoices").Append("goals-history").Append("achievements");

    private bool TryScreen(string screen, out string path, out string labelKey)
    {
        var id = (screen ?? string.Empty).Trim().ToLowerInvariant();

        if (ExtraScreens.TryGetValue(id, out var extra))
        {
            path = extra.Path;
            labelKey = extra.LabelKey;
            return true;
        }

        var guide = _assistantTree.GuideScreens.FirstOrDefault(s => s.Id == $"guide-{id}");

        path = guide?.Path ?? string.Empty;
        labelKey = guide?.LabelKey ?? string.Empty;

        return guide is not null;
    }

    private static bool TryDate(string? value, out DateTime date) =>
        DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    private static string Iso(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static decimal Round(decimal value) => Math.Round(value, 2);

    private sealed record SearchRow(
        DateTime Date,
        string Description,
        decimal Amount,
        string Kind,
        int? CategoryId,
        bool FromCard);
}
