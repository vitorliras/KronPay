using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Goals;
using Domain.Interfaces;
using Domain.Interfaces.Goals;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Goals;

public sealed class RegisterFinancialGoalContributionUseCase
    : IUseCase<RegisterContributionRequest, ContributionResponse>
{
    private readonly IFinancialGoalRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public RegisterFinancialGoalContributionUseCase(
        IFinancialGoalRepository repository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<ContributionResponse>> ExecuteAsync(RegisterContributionRequest request)
    {
        var userId = _currentUser.UserId;

        var goal = await _repository.GetByIdAsync(request.GoalId, userId);
        if (goal is null)
            return ResultEntity<ContributionResponse>.Failure(MessageKeys.GoalNotFound);

        goal.RegisterContribution(request.Amount);

        if (!_repository.Update(goal))
            return ResultEntity<ContributionResponse>.Failure(MessageKeys.UpdateFailed);

        if (!await _uow.CommitAsync())
            return ResultEntity<ContributionResponse>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<ContributionResponse>.Success(
            new ContributionResponse(goal.CurrentAmount, goal.Status),
            MessageKeys.ContributionRegistered);
    }
}
