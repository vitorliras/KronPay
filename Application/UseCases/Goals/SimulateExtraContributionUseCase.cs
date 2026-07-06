using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Goals;
using Domain.Interfaces.Goals;
using Domain.Services.Goals;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Goals;

public sealed class SimulateExtraContributionUseCase
    : IUseCase<SimulateExtraContributionRequest, SimulateExtraContributionResponse>
{
    private readonly IFinancialGoalRepository _repository;
    private readonly IGoalContributionCalculator _calculator;
    private readonly ICurrentUserService _currentUser;

    public SimulateExtraContributionUseCase(
        IFinancialGoalRepository repository,
        IGoalContributionCalculator calculator,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _calculator = calculator;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<SimulateExtraContributionResponse>> ExecuteAsync(SimulateExtraContributionRequest request)
    {
        var userId = _currentUser.UserId;

        var goal = await _repository.GetByIdAsync(request.GoalId, userId);
        if (goal is null)
            return ResultEntity<SimulateExtraContributionResponse>.Failure(MessageKeys.GoalNotFound);

        var now = DateTime.UtcNow;
        var recommended = _calculator.RecommendedMonthlyContribution(goal, now);
        var accelerated = recommended + request.ExtraMonthlyAmount;

        DateTime? CompletionDate(decimal contribution)
        {
            if (contribution <= 0)
                return null;

            var remaining = goal.TargetAmount - goal.CurrentAmount;
            var monthsNeeded = (int)Math.Ceiling(remaining / contribution);
            return new DateTime(now.Year, now.Month, 1).AddMonths(monthsNeeded);
        }

        var originalDate = CompletionDate(recommended);
        var acceleratedDate = CompletionDate(accelerated);

        var daysAccelerated = originalDate is not null && acceleratedDate is not null
            ? (int)(originalDate.Value - acceleratedDate.Value).TotalDays
            : 0;

        var response = new SimulateExtraContributionResponse(originalDate, acceleratedDate, daysAccelerated);

        return ResultEntity<SimulateExtraContributionResponse>.Success(response, MessageKeys.OperationSuccess);
    }
}
