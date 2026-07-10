using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Goals;
using Application.Notifications;
using Domain.Enums.Gamification;
using Domain.Enums.Goals;
using Domain.Enums.Notifications;
using Domain.Interfaces;
using Domain.Interfaces.Goals;
using Domain.Services.Gamification;
using Microsoft.Extensions.Logging;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Goals;

public sealed class RegisterFinancialGoalContributionUseCase
    : IUseCase<RegisterContributionRequest, ContributionResponse>
{
    private readonly IFinancialGoalRepository _repository;
    private readonly INotificationService _notificationService;
    private readonly IGamificationService _gamificationService;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<RegisterFinancialGoalContributionUseCase> _logger;

    public RegisterFinancialGoalContributionUseCase(
        IFinancialGoalRepository repository,
        INotificationService notificationService,
        IGamificationService gamificationService,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        ILogger<RegisterFinancialGoalContributionUseCase> logger)
    {
        _repository = repository;
        _notificationService = notificationService;
        _gamificationService = gamificationService;
        _uow = uow;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ResultEntity<ContributionResponse>> ExecuteAsync(RegisterContributionRequest request)
    {
        var userId = _currentUser.UserId;

        var goal = await _repository.GetByIdAsync(request.GoalId, userId);
        if (goal is null)
            return ResultEntity<ContributionResponse>.Failure(MessageKeys.GoalNotFound);

        goal.RegisterContribution(request.Amount);

        await _notificationService.ResolveByRelatedEntityAsync(userId, "FinancialGoal", goal.Id);

        if (goal.Status == FinancialGoalStatus.Completed)
        {
            var payload = new Dictionary<string, string> { ["goalName"] = goal.Description };

            await _notificationService.CreateInstantAsync(
                userId,
                NotificationType.FinancialGoalCompleted,
                NotificationCriticality.Informative,
                MessageKeys.NotificationFinancialGoalCompleted,
                payload,
                "FinancialGoal",
                goal.Id);
        }

        if (!_repository.Update(goal))
            return ResultEntity<ContributionResponse>.Failure(MessageKeys.UpdateFailed);

        if (!await _uow.CommitAsync())
            return ResultEntity<ContributionResponse>.Failure(MessageKeys.DataPersistenceFailed);

        await TriggerGamificationBestEffort(userId, goal.Id);

        return ResultEntity<ContributionResponse>.Success(
            new ContributionResponse(goal.CurrentAmount, goal.Status),
            MessageKeys.ContributionRegistered);
    }

    private async Task TriggerGamificationBestEffort(int userId, int goalId)
    {
        try
        {
            await _gamificationService.TriggerInstantEvaluationAsync(userId, MissionEventType.GoalAchieved, goalId);

            if (!await _uow.CommitAsync())
                _logger.LogError(
                    "Falha ao persistir a avaliação instantânea de gamificação para o usuário {UserId}.", userId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha inesperada ao avaliar gamificação instantânea para o usuário {UserId}.", userId);
        }
    }
}
