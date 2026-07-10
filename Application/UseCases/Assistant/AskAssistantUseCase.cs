using System.Globalization;
using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Assistant;
using Application.Services.Assistant;
using Domain.Entities.Card;
using Domain.Entities.Goals;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Domain.Interfaces.Transactions;
using Domain.Services.Gamification;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Shared.Localization;
using Shared.Resources;
using Shared.Results;

namespace Application.UseCases.Assistant;

public sealed class AskAssistantUseCase : IUseCase<AskAssistantRequest, AssistantNodeResponse>
{
    private readonly ICurrentUserService _currentUser;
    private readonly AssistantTree _tree;
    private readonly UserDataRichnessChecker _richnessChecker;
    private readonly CategoryParameterResolver _categoryResolver;
    private readonly TopicDisambiguationResolver _topicResolver;
    private readonly GoalAssistantResolver _goalResolver;
    private readonly CardAssistantResolver _cardResolver;
    private readonly NotificationAssistantResolver _notificationResolver;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICardInvoiceRepository _cardInvoiceRepository;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly IGamificationService _gamificationService;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<AskAssistantUseCase> _logger;

    public AskAssistantUseCase(
        ICurrentUserService currentUser,
        AssistantTree tree,
        UserDataRichnessChecker richnessChecker,
        CategoryParameterResolver categoryResolver,
        TopicDisambiguationResolver topicResolver,
        GoalAssistantResolver goalResolver,
        CardAssistantResolver cardResolver,
        NotificationAssistantResolver notificationResolver,
        ITransactionRepository transactionRepository,
        ICardInvoiceRepository cardInvoiceRepository,
        IStringLocalizer<Messages> localizer,
        IGamificationService gamificationService,
        IUnitOfWork uow,
        ILogger<AskAssistantUseCase> logger)
    {
        _currentUser = currentUser;
        _tree = tree;
        _richnessChecker = richnessChecker;
        _categoryResolver = categoryResolver;
        _topicResolver = topicResolver;
        _goalResolver = goalResolver;
        _localizer = localizer;
        _cardResolver = cardResolver;
        _notificationResolver = notificationResolver;
        _transactionRepository = transactionRepository;
        _cardInvoiceRepository = cardInvoiceRepository;
        _gamificationService = gamificationService;
        _uow = uow;
        _logger = logger;
    }

    public async Task<ResultEntity<AssistantNodeResponse>> ExecuteAsync(AskAssistantRequest request)
    {
        var userId = _currentUser.UserId;
        var currentNodeId = string.IsNullOrWhiteSpace(request.CurrentNodeId) ? AssistantTree.Root : request.CurrentNodeId;

        if (request.SelectedOptionId == "back")
        {
            var parentId = _tree.GetParent(currentNodeId) ?? AssistantTree.Root;
            return Success(await RenderNodeAsync(parentId, userId));
        }

        if (!string.IsNullOrWhiteSpace(request.FreeText))
        {
            var freeTextResponse = await ResolveFreeTextAsync(currentNodeId, request.FreeText, userId);
            await TriggerGamificationBestEffort(userId);
            return Success(freeTextResponse);
        }

        if (!string.IsNullOrWhiteSpace(request.SelectedOptionId))
        {
            var optionResponse = await ResolveOptionAsync(request.SelectedOptionId, userId);
            await TriggerGamificationBestEffort(userId);
            return Success(optionResponse);
        }

        return Success(await RenderNodeAsync(currentNodeId, userId));
    }

    private async Task TriggerGamificationBestEffort(int userId)
    {
        try
        {
            await _gamificationService.NotifyAssistantConversationAsync(userId);

            if (!await _uow.CommitAsync())
                _logger.LogError(
                    "Falha ao persistir a avaliação instantânea de gamificação para o usuário {UserId}.", userId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha inesperada ao avaliar gamificação instantânea para o usuário {UserId}.", userId);
        }
    }

    private async Task<AssistantNodeResponse> ResolveOptionAsync(string optionId, int userId)
    {
        if (optionId.StartsWith("category:", StringComparison.Ordinal) ||
            optionId.StartsWith("categoryitem:", StringComparison.Ordinal))
        {
            var match = await _categoryResolver.ResolveByOptionIdAsync(optionId, userId);
            return await BuildCategoryAnswerAsync(match, userId);
        }

        if (optionId == "categories:more")
            return await RenderNodeAsync(AssistantTree.TransactionsCategorySearch, userId);

        if (optionId.StartsWith("goal:", StringComparison.Ordinal))
        {
            var goalId = ParseId(optionId, "goal:");
            var goal = goalId is null ? null : await _goalResolver.GetByIdAsync(goalId.Value, userId);
            return goal is null
                ? await RenderNodeAsync(AssistantTree.TopicGoals, userId, MessageKeys.AssistantFallbackMessage)
                : BuildGoalDetailAnswer(goal);
        }

        if (optionId.StartsWith("card:", StringComparison.Ordinal))
        {
            var cardId = ParseId(optionId, "card:");
            var card = cardId is null ? null : await _cardResolver.GetByIdAsync(cardId.Value, userId);
            return card is null
                ? await RenderNodeAsync(AssistantTree.TopicCard, userId, MessageKeys.AssistantFallbackMessage)
                : await BuildCardDetailAnswerAsync(card, userId);
        }

        if (optionId.StartsWith("notification:", StringComparison.Ordinal))
        {
            var notificationId = ParseId(optionId, "notification:");
            if (notificationId is not null)
                await _notificationResolver.MarkAsReadAsync(notificationId.Value, userId);

            return await RenderNotificationsListOrDoneAsync(userId);
        }

        if (optionId == "notifications:mark-all")
        {
            var confirmOptions = new[]
            {
                new AssistantOptionResponse("notifications:mark-all:confirm", MessageKeys.AssistantOptionConfirmMarkAllAsRead, Array.Empty<string>()),
            };
            return Finalize(AssistantTree.NotificationsList, MessageKeys.AssistantMarkAllConfirmMessage, Array.Empty<string>(), confirmOptions, isFinal: false);
        }

        if (optionId == "notifications:mark-all:confirm")
        {
            await _notificationResolver.MarkAllAsReadAsync(userId);
            return Finalize(AssistantTree.TopicNotifications, MessageKeys.AllCaughtUp, Array.Empty<string>(), Array.Empty<AssistantOptionResponse>(), isFinal: true);
        }

        var guideScreen = _tree.GuideScreens.FirstOrDefault(s => s.Id == optionId);
        if (guideScreen is not null)
            return BuildGuideNavigation(guideScreen);

        return await RenderNodeAsync(optionId, userId);
    }

    private async Task<AssistantNodeResponse> ResolveFreeTextAsync(string currentNodeId, string freeText, int userId)
    {
        if (currentNodeId is AssistantTree.TransactionsCategorySelect or AssistantTree.TransactionsCategorySearch)
        {
            var match = await _categoryResolver.ResolveAsync(freeText, userId);
            if (match is not null)
                return await BuildCategoryAnswerAsync(match, userId);

            return await RenderNodeAsync(currentNodeId, userId, MessageKeys.AssistantFallbackMessage);
        }

        var localTopics = LocalCandidates(currentNodeId);
        if (localTopics.Count > 0)
        {
            var localResolution = _topicResolver.Resolve(freeText, localTopics);
            if (localResolution.ClarifyOptions is not null)
                return BuildClarify(currentNodeId, localResolution.ClarifyOptions);
            if (localResolution.ResolvedNodeId is not null)
                return await RenderNodeAsync(localResolution.ResolvedNodeId, userId);
        }

        var topicResolution = _topicResolver.Resolve(freeText, _tree.Topics);
        if (topicResolution.ClarifyOptions is not null)
            return BuildClarify(currentNodeId, topicResolution.ClarifyOptions);
        if (topicResolution.ResolvedNodeId is not null)
            return await RenderNodeAsync(topicResolution.ResolvedNodeId, userId);

        return await RenderNodeAsync(currentNodeId, userId, MessageKeys.AssistantFallbackMessage);
    }

    private AssistantNodeResponse BuildClarify(string currentNodeId, IReadOnlyList<AssistantOptionResponse> options) =>
        Finalize(currentNodeId, MessageKeys.AssistantTopicClarifyMessage, Array.Empty<string>(), options, isFinal: false);

    private IReadOnlyList<AssistantTopic> LocalCandidates(string nodeId) => nodeId switch
    {
        AssistantTree.Root or AssistantTree.Guide => _tree.Topics,
        AssistantTree.TopicTransactions => _tree.TransactionsSubTopics,
        _ => Array.Empty<AssistantTopic>(),
    };

    private async Task<AssistantNodeResponse> RenderNodeAsync(string nodeId, int userId, string? fallbackMessageKey = null)
    {
        var response = await BuildNodeAsync(nodeId, userId);
        return fallbackMessageKey is null ? response : response with { MessageKey = fallbackMessageKey };
    }

    private async Task<AssistantNodeResponse> BuildNodeAsync(string nodeId, int userId) => nodeId switch
    {
        AssistantTree.Root => await BuildRootAsync(userId),
        AssistantTree.Guide => BuildGuide(),
        AssistantTree.TopicTransactions => BuildStatic(AssistantTree.TopicTransactions, MessageKeys.AssistantTransactionsTopicMessage, SubTopicOptions()),
        AssistantTree.TransactionsRecent => await BuildRecentTransactionsAsync(userId),
        AssistantTree.TransactionsCategorySelect => await BuildCategorySelectAsync(userId),
        AssistantTree.TransactionsCategorySearch => BuildStatic(AssistantTree.TransactionsCategorySearch, MessageKeys.AssistantCategorySearchMessage, Array.Empty<AssistantOptionResponse>()),
        AssistantTree.TopicGoals => await BuildGoalsAsync(userId),
        AssistantTree.TopicCard => await BuildCardAsync(userId),
        AssistantTree.TopicNotifications => await BuildNotificationsAsync(userId),
        AssistantTree.NotificationsList => await RenderNotificationsListOrDoneAsync(userId),
        _ => await BuildRootAsync(userId),
    };

    private async Task<AssistantNodeResponse> BuildRootAsync(int userId)
    {
        var hasEnoughData = await _richnessChecker.HasEnoughDataAsync(userId);
        var options = new List<AssistantOptionResponse>();

        if (hasEnoughData)
            options.Add(new AssistantOptionResponse(AssistantTree.Guide, MessageKeys.AssistantOptionGuidedTour, Array.Empty<string>()));

        options.AddRange(TopicOptions());

        var messageKey = hasEnoughData ? MessageKeys.AssistantRootRichDataMessage : MessageKeys.AssistantRootLowDataMessage;
        return Finalize(AssistantTree.Root, messageKey, Array.Empty<string>(), options, isFinal: false);
    }

    private AssistantNodeResponse BuildGuide()
    {
        var options = _tree.GuideScreens
            .Select(s => new AssistantOptionResponse(s.Id, s.LabelKey, Array.Empty<string>()))
            .ToList();

        return Finalize(AssistantTree.Guide, MessageKeys.AssistantGuideMessage, Array.Empty<string>(), options, isFinal: false);
    }

    private AssistantNodeResponse BuildGuideNavigation(GuideScreen screen)
    {
        var navigateTo = new AssistantNavigationResponse(
            screen.Path,
            new Dictionary<string, string> { ["tour"] = "true" },
            AutoNavigate: true,
            MessageKeys.AssistantOpenScreenButton);

        var screenName = _localizer[screen.LabelKey].Value;

        return Finalize(AssistantTree.Guide, MessageKeys.AssistantGuideNavigatingMessage, new[] { screenName }, Array.Empty<AssistantOptionResponse>(), isFinal: true)
            with
            { NavigateTo = new[] { navigateTo } };
    }

    private IReadOnlyList<AssistantOptionResponse> TopicOptions() =>
        _tree.Topics.Select(t => new AssistantOptionResponse(t.NodeId, t.LabelKey, Array.Empty<string>())).ToList();

    private IReadOnlyList<AssistantOptionResponse> SubTopicOptions() =>
        _tree.TransactionsSubTopics.Select(t => new AssistantOptionResponse(t.NodeId, t.LabelKey, Array.Empty<string>())).ToList();

    private AssistantNodeResponse BuildStatic(string nodeId, string messageKey, IReadOnlyList<AssistantOptionResponse> options) =>
        Finalize(nodeId, messageKey, Array.Empty<string>(), options, isFinal: false);

    private async Task<AssistantNodeResponse> BuildCategorySelectAsync(int userId)
    {
        var options = await _categoryResolver.BuildTopOptionsAsync(userId);
        return Finalize(AssistantTree.TransactionsCategorySelect, MessageKeys.AssistantCategorySelectMessage, Array.Empty<string>(), options, isFinal: false);
    }

    private async Task<AssistantNodeResponse> BuildCategoryAnswerAsync(CategoryParameterMatch? match, int userId)
    {
        if (match is null)
        {
            var options = await _categoryResolver.BuildTopOptionsAsync(userId);
            return Finalize(AssistantTree.TransactionsCategorySelect, MessageKeys.AssistantFallbackMessage, Array.Empty<string>(), options, isFinal: false);
        }

        var now = DateTime.UtcNow;
        var transactions = await _transactionRepository.GetByMonthAsync(userId, now.Year, now.Month);

        var total = transactions
            .Where(t => t.CodTypeTransaction == "E" && t.Status != "C" && t.CategoryId == match.CategoryId
                && (match.CategoryItemId is null || t.CategoryItemId == match.CategoryItemId))
            .Sum(t => t.Amount);

        var args = new[] { FormatCurrency(total), match.DisplayName };
        return Finalize(AssistantTree.TransactionsCategorySelect, MessageKeys.AssistantCategorySpendingAnswer, args, Array.Empty<AssistantOptionResponse>(), isFinal: true);
    }

    private async Task<AssistantNodeResponse> BuildRecentTransactionsAsync(int userId)
    {
        var now = DateTime.UtcNow;
        var transactions = (await _transactionRepository.GetByMonthAsync(userId, now.Year, now.Month))
            .Where(t => t.Status != "C")
            .OrderByDescending(t => t.TransactionDate)
            .Take(5)
            .ToList();

        if (transactions.Count == 0)
            return Finalize(AssistantTree.TransactionsRecent, MessageKeys.AssistantNoRecentTransactionsAnswer, Array.Empty<string>(), Array.Empty<AssistantOptionResponse>(), isFinal: true);

        var summary = string.Join(", ", transactions.Select(t => $"{t.Description} ({FormatCurrency(t.Amount)})"));
        return Finalize(AssistantTree.TransactionsRecent, MessageKeys.AssistantRecentTransactionsAnswer, new[] { summary }, Array.Empty<AssistantOptionResponse>(), isFinal: true);
    }

    private async Task<AssistantNodeResponse> BuildGoalsAsync(int userId)
    {
        var goals = await _goalResolver.GetActiveGoalsAsync(userId);

        if (goals.Count == 0)
            return Finalize(AssistantTree.TopicGoals, MessageKeys.AssistantNoActiveGoalsAnswer, Array.Empty<string>(), Array.Empty<AssistantOptionResponse>(), isFinal: true);

        if (goals.Count == 1)
            return BuildGoalDetailAnswer(goals[0]);

        var options = _goalResolver.BuildSelectOptions(goals);
        var args = new[] { goals.Count.ToString(CultureInfo.InvariantCulture) };
        return Finalize(AssistantTree.GoalsSelect, MessageKeys.AssistantGoalsSelectMessage, args, options, isFinal: false);
    }

    private AssistantNodeResponse BuildGoalDetailAnswer(FinancialGoal goal)
    {
        var percent = goal.TargetAmount == 0 ? 0 : Math.Round(goal.CurrentAmount / goal.TargetAmount * 100, 0);
        var args = new[]
        {
            goal.Description,
            FormatCurrency(goal.CurrentAmount),
            FormatCurrency(goal.TargetAmount),
            percent.ToString(CultureInfo.InvariantCulture),
            goal.TargetDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        };

        var navigateTo = new AssistantNavigationResponse(
            "/goals",
            new Dictionary<string, string> { ["goalId"] = goal.Id.ToString(CultureInfo.InvariantCulture) },
            AutoNavigate: false,
            MessageKeys.AssistantOpenScreenButton);

        return Finalize(AssistantTree.TopicGoals, MessageKeys.AssistantGoalDetailAnswer, args, Array.Empty<AssistantOptionResponse>(), isFinal: true)
            with
            { NavigateTo = new[] { navigateTo } };
    }

    private async Task<AssistantNodeResponse> BuildCardAsync(int userId)
    {
        var cards = await _cardResolver.GetActiveCardsAsync(userId);

        if (cards.Count == 0)
            return Finalize(AssistantTree.TopicCard, MessageKeys.AssistantNoCardAnswer, Array.Empty<string>(), Array.Empty<AssistantOptionResponse>(), isFinal: true);

        if (cards.Count == 1)
            return await BuildCardDetailAnswerAsync(cards[0], userId);

        var options = _cardResolver.BuildSelectOptions(cards);
        var args = new[] { cards.Count.ToString(CultureInfo.InvariantCulture) };
        return Finalize(AssistantTree.CardSelect, MessageKeys.AssistantCardSelectMessage, args, options, isFinal: false);
    }

    private async Task<AssistantNodeResponse> BuildCardDetailAnswerAsync(CreditCard card, int userId)
    {
        var invoices = await _cardInvoiceRepository.GetByCardAsync(card.Id, userId);
        var openInvoice = invoices
            .Where(i => !i.IsPaid)
            .OrderBy(i => i.DueDate)
            .FirstOrDefault();

        var cardIdParam = new Dictionary<string, string> { ["cardId"] = card.Id.ToString(CultureInfo.InvariantCulture) };
        var navigateTo = new[]
        {
            new AssistantNavigationResponse("/credit-card/invoices", cardIdParam, AutoNavigate: false, MessageKeys.AssistantOptionViewInvoice),
            new AssistantNavigationResponse("/credit-card/cards", cardIdParam, AutoNavigate: false, MessageKeys.AssistantOptionViewCardRegistration),
        };

        if (openInvoice is null)
            return Finalize(AssistantTree.TopicCard, MessageKeys.AssistantNoCardAnswer, Array.Empty<string>(), Array.Empty<AssistantOptionResponse>(), isFinal: true)
                with
                { NavigateTo = navigateTo };

        var args = new[] { openInvoice.DueDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), FormatCurrency(openInvoice.TotalAmount) };
        return Finalize(AssistantTree.TopicCard, MessageKeys.AssistantCardTopicAnswer, args, Array.Empty<AssistantOptionResponse>(), isFinal: true)
            with
            { NavigateTo = navigateTo };
    }

    private async Task<AssistantNodeResponse> BuildNotificationsAsync(int userId)
    {
        var unreadCount = await _notificationResolver.GetUnreadCountAsync(userId);
        var args = new[] { unreadCount.ToString(CultureInfo.InvariantCulture) };

        var options = new List<AssistantOptionResponse>();
        if (unreadCount > 0)
            options.Add(new AssistantOptionResponse(AssistantTree.NotificationsList, MessageKeys.AssistantOptionViewNotifications, Array.Empty<string>()));

        return Finalize(AssistantTree.TopicNotifications, MessageKeys.AssistantNotificationsTopicAnswer, args, options, isFinal: options.Count == 0);
    }

    private async Task<AssistantNodeResponse> RenderNotificationsListOrDoneAsync(int userId)
    {
        var options = await _notificationResolver.BuildListOptionsAsync(userId);
        if (options.Count == 0)
            return Finalize(AssistantTree.TopicNotifications, MessageKeys.AllCaughtUp, Array.Empty<string>(), Array.Empty<AssistantOptionResponse>(), isFinal: true);

        return Finalize(AssistantTree.NotificationsList, MessageKeys.AssistantNotificationsListMessage, Array.Empty<string>(), options, isFinal: false);
    }

    private AssistantNodeResponse Finalize(
        string nodeId,
        string messageKey,
        IReadOnlyCollection<string> messageArgs,
        IReadOnlyCollection<AssistantOptionResponse> options,
        bool isFinal)
    {
        var finalOptions = new List<AssistantOptionResponse>(options)
        {
            new("free-text", MessageKeys.AssistantOptionFreeText, Array.Empty<string>()),
        };

        if (_tree.GetParent(nodeId) is not null)
            finalOptions.Add(new AssistantOptionResponse("back", MessageKeys.AssistantOptionBack, Array.Empty<string>()));

        return new AssistantNodeResponse(nodeId, messageKey, messageArgs, finalOptions, isFinal);
    }

    private static int? ParseId(string optionId, string prefix) =>
        int.TryParse(optionId.AsSpan(prefix.Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? id : null;

    private static string FormatCurrency(decimal amount) =>
        amount.ToString("C2", CultureInfo.GetCultureInfo("pt-BR"));

    private static ResultEntity<AssistantNodeResponse> Success(AssistantNodeResponse response) =>
        ResultEntity<AssistantNodeResponse>.Success(response, MessageKeys.OperationSuccess);
}
