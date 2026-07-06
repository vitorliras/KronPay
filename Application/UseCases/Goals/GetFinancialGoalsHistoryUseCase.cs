using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Goals;
using Domain.Interfaces.Goals;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Goals;

public sealed class GetFinancialGoalsHistoryUseCase
    : IUseCase<GetGoalsHistoryRequest, GoalHistoryResponse>
{
    private readonly IFinancialGoalRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetFinancialGoalsHistoryUseCase(
        IFinancialGoalRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<GoalHistoryResponse>> ExecuteAsync(GetGoalsHistoryRequest request)
    {
        var userId = _currentUser.UserId;

        var goals = await _repository.GetHistoryAsync(userId, request.Search);

        var response = new GoalHistoryResponse(
            goals.Select(g => new FinancialGoalResponse(
                g.Id,
                g.Description,
                g.TargetAmount,
                g.CurrentAmount,
                g.TargetDate,
                g.Priority,
                g.Status,
                g.CreatedAt,
                g.CompletedAt,
                g.PreviousAttemptGoalId))
            .ToList());

        return ResultEntity<GoalHistoryResponse>.Success(response, MessageKeys.OperationSuccess);
    }
}
