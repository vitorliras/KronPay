using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Goals;
using Application.Planning;
using Domain.Enums.Goals;
using Domain.Interfaces.Goals;
using Domain.Services.Goals;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Goals;

public sealed class GetFinancialGoalDetailUseCase
    : IUseCase<GetFinancialGoalDetailRequest, GoalViabilityResponse>
{
    private readonly IFinancialGoalRepository _repository;
    private readonly IProjectionRunner _runner;
    private readonly IGoalContributionCalculator _calculator;
    private readonly ICurrentUserService _currentUser;

    public GetFinancialGoalDetailUseCase(
        IFinancialGoalRepository repository,
        IProjectionRunner runner,
        IGoalContributionCalculator calculator,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _runner = runner;
        _calculator = calculator;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<GoalViabilityResponse>> ExecuteAsync(GetFinancialGoalDetailRequest request)
    {
        var userId = _currentUser.UserId;

        var goal = await _repository.GetByIdAsync(request.GoalId, userId);
        if (goal is null)
            return ResultEntity<GoalViabilityResponse>.Failure(MessageKeys.GoalNotFound);

        var now = DateTime.UtcNow;
        var recommended = _calculator.RecommendedMonthlyContribution(goal, now);
        var monthsRemaining = _calculator.MonthsRemaining(goal, now);
        var horizon = Math.Clamp(monthsRemaining + 3, PlanningDefaults.DefaultHorizonMonths, PlanningDefaults.MaxHorizonMonths);

        var baseline = await _runner.RunAsync(userId, now, horizon, null);
        var reserve = baseline.Parameters.SafetyReserve;
        var months = baseline.Projection.Months;

        decimal? minRoomPerMonth = null;
        for (var i = 0; i < months.Count; i++)
        {
            var room = (months[i].ProbableClosing - reserve) / (i + 1);
            if (minRoomPerMonth is null || room < minRoomPerMonth)
                minRoomPerMonth = room;
        }

        var fastAmount = Math.Max(0, recommended + (minRoomPerMonth ?? 0));
        var conservativeAmount = fastAmount / 2m;

        decimal SafetyImpact(decimal contribution)
        {
            var worst = decimal.MaxValue;
            for (var i = 0; i < months.Count; i++)
            {
                var adjusted = months[i].ProbableClosing + (recommended - contribution) * (i + 1);
                var impact = adjusted - reserve;
                if (impact < worst)
                    worst = impact;
            }

            return worst;
        }

        DateTime? CompletionDate(decimal contribution)
        {
            if (contribution <= 0)
                return null;

            var remaining = goal.TargetAmount - goal.CurrentAmount;
            var monthsNeeded = (int)Math.Ceiling(remaining / contribution);
            return new DateTime(now.Year, now.Month, 1).AddMonths(monthsNeeded);
        }

        var recommendedOption = new GoalStrategyOptionResponse(
            GoalStrategyLabel.Recommended, recommended, CompletionDate(recommended), SafetyImpact(recommended));

        var conservativeOption = new GoalStrategyOptionResponse(
            GoalStrategyLabel.Conservative, conservativeAmount, CompletionDate(conservativeAmount), SafetyImpact(conservativeAmount));

        var fastOption = new GoalStrategyOptionResponse(
            GoalStrategyLabel.Fast, fastAmount, CompletionDate(fastAmount), SafetyImpact(fastAmount));

        var atRisk = recommendedOption.SafetyReserveImpact < 0;

        var response = new GoalViabilityResponse(
            atRisk,
            new[] { recommendedOption, conservativeOption, fastOption });

        return ResultEntity<GoalViabilityResponse>.Success(response, MessageKeys.OperationSuccess);
    }
}
