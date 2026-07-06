using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Goals;
using Domain.Entities.Goals;
using Domain.Interfaces;
using Domain.Interfaces.Goals;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Goals;

public sealed class CreateFinancialGoalUseCase
    : IUseCase<CreateFinancialGoalRequest, FinancialGoalResponse>
{
    private readonly IFinancialGoalRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateFinancialGoalUseCase(
        IFinancialGoalRepository repository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<FinancialGoalResponse>> ExecuteAsync(CreateFinancialGoalRequest request)
    {
        var userId = _currentUser.UserId;

        var goal = new FinancialGoal(
            userId,
            request.Description,
            request.TargetAmount,
            request.TargetDate,
            request.Priority);

        if (!await _repository.AddAsync(goal))
            return ResultEntity<FinancialGoalResponse>.Failure(MessageKeys.InsertFalied);

        if (!await _uow.CommitAsync())
            return ResultEntity<FinancialGoalResponse>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<FinancialGoalResponse>.Success(
            new FinancialGoalResponse(
                goal.Id,
                goal.Description,
                goal.TargetAmount,
                goal.CurrentAmount,
                goal.TargetDate,
                goal.Priority,
                goal.Status,
                goal.CreatedAt,
                goal.CompletedAt,
                goal.PreviousAttemptGoalId),
            MessageKeys.GoalCreated);
    }
}
