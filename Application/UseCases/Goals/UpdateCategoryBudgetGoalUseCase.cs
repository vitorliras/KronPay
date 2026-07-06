using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Goals;
using Domain.Interfaces;
using Domain.Interfaces.Goals;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Goals;

public sealed class UpdateCategoryBudgetGoalUseCase
    : IUseCase<UpdateCategoryBudgetGoalRequest, CategoryBudgetGoalResponse>
{
    private readonly ICategoryBudgetGoalRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateCategoryBudgetGoalUseCase(
        ICategoryBudgetGoalRepository repository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<CategoryBudgetGoalResponse>> ExecuteAsync(UpdateCategoryBudgetGoalRequest request)
    {
        var userId = _currentUser.UserId;

        var goal = await _repository.GetByIdAsync(request.Id, userId);
        if (goal is null)
            return ResultEntity<CategoryBudgetGoalResponse>.Failure(MessageKeys.GoalNotFound);

        goal.UpdateLimit(request.MonthlyLimit);
        goal.UpdatePriority(request.Priority);

        if (!_repository.Update(goal))
            return ResultEntity<CategoryBudgetGoalResponse>.Failure(MessageKeys.UpdateFailed);

        if (!await _uow.CommitAsync())
            return ResultEntity<CategoryBudgetGoalResponse>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<CategoryBudgetGoalResponse>.Success(
            new CategoryBudgetGoalResponse(
                goal.Id,
                goal.CategoryId,
                goal.MonthlyLimit,
                goal.Priority,
                goal.Active,
                CurrentPeriodSpent: 0),
            MessageKeys.CategoryBudgetGoalUpdated);
    }
}
