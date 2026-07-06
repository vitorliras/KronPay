using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Goals;
using Domain.Entities.Goals;
using Domain.Interfaces;
using Domain.Interfaces.Goals;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Goals;

public sealed class CreateCategoryBudgetGoalUseCase
    : IUseCase<CreateCategoryBudgetGoalRequest, CategoryBudgetGoalResponse>
{
    private readonly ICategoryBudgetGoalRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateCategoryBudgetGoalUseCase(
        ICategoryBudgetGoalRepository repository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<CategoryBudgetGoalResponse>> ExecuteAsync(CreateCategoryBudgetGoalRequest request)
    {
        var userId = _currentUser.UserId;

        var existing = await _repository.GetByCategoryIdAsync(request.CategoryId, userId);
        if (existing is not null)
            return ResultEntity<CategoryBudgetGoalResponse>.Failure(MessageKeys.ExistsAnotherRegister);

        var goal = new CategoryBudgetGoal(userId, request.CategoryId, request.MonthlyLimit, request.Priority);

        if (!await _repository.AddAsync(goal))
            return ResultEntity<CategoryBudgetGoalResponse>.Failure(MessageKeys.InsertFalied);

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
            MessageKeys.CategoryBudgetGoalCreated);
    }
}
