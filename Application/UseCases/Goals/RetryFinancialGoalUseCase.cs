using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Goals;
using Domain.Entities.Goals;
using Domain.Enums.Goals;
using Domain.Interfaces;
using Domain.Interfaces.Goals;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Goals;

public sealed class RetryFinancialGoalUseCase
    : IUseCase<RetryFinancialGoalRequest, FinancialGoalResponse>
{
    private readonly IFinancialGoalRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public RetryFinancialGoalUseCase(
        IFinancialGoalRepository repository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<FinancialGoalResponse>> ExecuteAsync(RetryFinancialGoalRequest request)
    {
        var userId = _currentUser.UserId;

        var previousGoal = await _repository.GetByIdAsync(request.GoalId, userId);
        if (previousGoal is null)
            return ResultEntity<FinancialGoalResponse>.Failure(MessageKeys.GoalNotFound);

        if (previousGoal.Status == FinancialGoalStatus.Active)
            return ResultEntity<FinancialGoalResponse>.Failure(MessageKeys.GoalNotEligibleForRetry);

        var newGoal = new FinancialGoal(
            userId,
            previousGoal.Description,
            previousGoal.TargetAmount,
            request.NewTargetDate,
            previousGoal.Priority,
            previousGoal.Id);

        if (!await _repository.AddAsync(newGoal))
            return ResultEntity<FinancialGoalResponse>.Failure(MessageKeys.InsertFalied);

        if (!await _uow.CommitAsync())
            return ResultEntity<FinancialGoalResponse>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<FinancialGoalResponse>.Success(
            new FinancialGoalResponse(
                newGoal.Id,
                newGoal.Description,
                newGoal.TargetAmount,
                newGoal.CurrentAmount,
                newGoal.TargetDate,
                newGoal.Priority,
                newGoal.Status,
                newGoal.CreatedAt,
                newGoal.CompletedAt,
                newGoal.PreviousAttemptGoalId),
            MessageKeys.GoalRetried);
    }
}
