using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Goals;
using Domain.Interfaces;
using Domain.Interfaces.Goals;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Goals;

public sealed class DeactivateCategoryBudgetGoalUseCase
    : IUseCase<DeactivateCategoryBudgetGoalRequest, Unit>
{
    private readonly ICategoryBudgetGoalRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeactivateCategoryBudgetGoalUseCase(
        ICategoryBudgetGoalRepository repository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<Unit>> ExecuteAsync(DeactivateCategoryBudgetGoalRequest request)
    {
        var userId = _currentUser.UserId;

        var goal = await _repository.GetByIdAsync(request.Id, userId);
        if (goal is null)
            return ResultEntity<Unit>.Failure(MessageKeys.GoalNotFound);

        goal.Deactivate();

        if (!_repository.Update(goal))
            return ResultEntity<Unit>.Failure(MessageKeys.UpdateFailed);

        if (!await _uow.CommitAsync())
            return ResultEntity<Unit>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<Unit>.Success(Unit.Value, MessageKeys.CategoryBudgetGoalDeactivated);
    }
}
