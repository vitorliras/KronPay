using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Planning;
using Application.Planning;
using Domain.Services.Planning;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Planning;

public sealed class GetFinancialProjectionUseCase
    : IUseCase<GetFinancialProjectionRequest, FinancialProjectionResponse>
{
    private readonly IProjectionRunner _runner;
    private readonly IViabilityEvaluator _evaluator;
    private readonly ICurrentUserService _currentUser;

    public GetFinancialProjectionUseCase(
        IProjectionRunner runner,
        IViabilityEvaluator evaluator,
        ICurrentUserService currentUser)
    {
        _runner = runner;
        _evaluator = evaluator;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<FinancialProjectionResponse>> ExecuteAsync(GetFinancialProjectionRequest request)
    {
        var userId = _currentUser.UserId;
        var horizon = PlanningDefaults.NormalizeHorizon(request.HorizonMonths);

        var context = await _runner.RunAsync(userId, DateTime.UtcNow, horizon, request.SafetyReserve);
        var viability = _evaluator.Evaluate(context.Projection, context.Parameters);

        var response = new FinancialProjectionResponse(
            context.Parameters.InitialBalance,
            context.Projection.FinalBalance,
            context.Parameters.SafetyReserve,
            context.Projection.FirstNegativeMonth?.Year,
            context.Projection.FirstNegativeMonth?.Month,
            PlanningResponseMapper.Map(viability),
            PlanningResponseMapper.MapMonths(context.Projection));

        return ResultEntity<FinancialProjectionResponse>.Success(response, MessageKeys.OperationSuccess);
    }
}
