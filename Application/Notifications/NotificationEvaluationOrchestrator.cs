using System.Text.Json;
using Application.Notifications.Rules;
using Domain.Entities.Notifications;
using Domain.Enums.Notifications;
using Domain.interfaces;
using Domain.Interfaces;
using Domain.Interfaces.Notifications;
using Microsoft.Extensions.Logging;

namespace Application.Notifications;

public sealed class NotificationEvaluationOrchestrator : INotificationEvaluationOrchestrator
{
    private const int ArchivedRetentionDays = 30;

    private readonly IUserRepository _users;
    private readonly INotificationRepository _notifications;
    private readonly ITransactionNotificationRuleEvaluator _transactionEvaluator;
    private readonly ICardInvoiceNotificationRuleEvaluator _cardInvoiceEvaluator;
    private readonly IGoalNotificationRuleEvaluator _goalEvaluator;
    private readonly IFinancialIntelligenceNotificationRuleEvaluator _financialIntelligenceEvaluator;
    private readonly IDataHygieneNotificationRuleEvaluator _dataHygieneEvaluator;
    private readonly INotificationEmailDispatcher _emailDispatcher;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<NotificationEvaluationOrchestrator> _logger;

    public NotificationEvaluationOrchestrator(
        IUserRepository users,
        INotificationRepository notifications,
        ITransactionNotificationRuleEvaluator transactionEvaluator,
        ICardInvoiceNotificationRuleEvaluator cardInvoiceEvaluator,
        IGoalNotificationRuleEvaluator goalEvaluator,
        IFinancialIntelligenceNotificationRuleEvaluator financialIntelligenceEvaluator,
        IDataHygieneNotificationRuleEvaluator dataHygieneEvaluator,
        INotificationEmailDispatcher emailDispatcher,
        IUnitOfWork uow,
        ILogger<NotificationEvaluationOrchestrator> logger)
    {
        _users = users;
        _notifications = notifications;
        _transactionEvaluator = transactionEvaluator;
        _cardInvoiceEvaluator = cardInvoiceEvaluator;
        _goalEvaluator = goalEvaluator;
        _financialIntelligenceEvaluator = financialIntelligenceEvaluator;
        _dataHygieneEvaluator = dataHygieneEvaluator;
        _emailDispatcher = emailDispatcher;
        _uow = uow;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        var today = DateTime.UtcNow.Date;
        var userIds = await _users.GetAllUserIdsAsync();
        var createdNotifications = new List<(Notification Notification, int UserId)>();

        foreach (var userId in userIds)
        {
            var candidates = await CollectCandidatesAsync(userId, today);

            foreach (var candidate in candidates)
            {
                var existing = await _notifications.GetExistingUnresolvedAsync(
                    userId, candidate.Type, candidate.RelatedEntityType, candidate.RelatedEntityId);

                if (existing is not null)
                    continue;

                var notification = new Notification(
                    userId,
                    candidate.Type,
                    candidate.Criticality,
                    candidate.MessageKey,
                    JsonSerializer.Serialize(candidate.Args),
                    candidate.RelatedEntityType,
                    candidate.RelatedEntityId);

                if (await _notifications.AddAsync(notification))
                    createdNotifications.Add((notification, userId));
            }

            await ResolveStaleAsync(userId, candidates);
        }

        var cutoff = today.AddDays(-ArchivedRetentionDays);
        var archived = await _notifications.GetArchivedOlderThanAsync(cutoff);
        if (archived.Count > 0)
            await _notifications.DeleteRangeAsync(archived);

        if (!await _uow.CommitAsync())
        {
            _logger.LogError("Falha ao persistir a avaliação diária de notificações ({Today}).", today);
            return;
        }

        foreach (var (notification, userId) in createdNotifications)
            await _emailDispatcher.DispatchAsync(notification, userId);
    }

    private async Task<List<NotificationCandidate>> CollectCandidatesAsync(int userId, DateTime today)
    {
        var candidates = new List<NotificationCandidate>();

        candidates.AddRange(await _transactionEvaluator.EvaluateAsync(userId, today));
        candidates.AddRange(await _cardInvoiceEvaluator.EvaluateAsync(userId, today));
        candidates.AddRange(await _goalEvaluator.EvaluateAsync(userId, today));
        candidates.AddRange(await _financialIntelligenceEvaluator.EvaluateAsync(userId, today));
        candidates.AddRange(await _dataHygieneEvaluator.EvaluateAsync(userId, today));

        return candidates;
    }

    private async Task ResolveStaleAsync(int userId, IReadOnlyList<NotificationCandidate> candidates)
    {
        var stateBasedTypes = Enum.GetValues<NotificationType>().Where(t => !NotificationTypeCatalog.IsEventBased(t));

        foreach (var type in stateBasedTypes)
        {
            var unresolved = await _notifications.GetUnresolvedByTypeAsync(userId, type);
            if (unresolved.Count == 0)
                continue;

            var stillValid = candidates
                .Where(c => c.Type == type)
                .Select(c => c.RelatedEntityId)
                .ToHashSet();

            foreach (var notification in unresolved)
            {
                if (stillValid.Contains(notification.RelatedEntityId))
                    continue;

                notification.Resolve();
                _notifications.Update(notification);
            }
        }
    }
}
