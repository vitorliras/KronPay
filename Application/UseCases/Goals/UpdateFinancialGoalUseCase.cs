using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Goals;
using Domain.Interfaces;
using Domain.Interfaces.Goals;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Goals;

public sealed class UpdateFinancialGoalUseCase
    : IUseCase<UpdateFinancialGoalRequest, FinancialGoalResponse>
{
    private readonly IFinancialGoalRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateFinancialGoalUseCase(
        IFinancialGoalRepository repository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<FinancialGoalResponse>> ExecuteAsync(UpdateFinancialGoalRequest request)
    {
        var userId = _currentUser.UserId;

        var goal = await _repository.GetByIdAsync(request.Id, userId);
        if (goal is null)
            return ResultEntity<FinancialGoalResponse>.Failure(MessageKeys.GoalNotFound);

        goal.UpdateDetails(request.Description, request.TargetAmount, request.TargetDate, request.Priority);

        if (!_repository.Update(goal))
            return ResultEntity<FinancialGoalResponse>.Failure(MessageKeys.UpdateFailed);

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
            MessageKeys.GoalUpdated);
    }
}
