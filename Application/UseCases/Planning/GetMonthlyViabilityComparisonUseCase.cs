using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Planning;
using Application.Planning;
using Domain.Entities.Card;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Domain.Services.Planning;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Planning;

public sealed class GetMonthlyViabilityComparisonUseCase
    : IUseCase<MonthlyViabilityComparisonRequest, MonthlyViabilityComparisonResponse>
{
    private readonly IProjectionRunner _runner;
    private readonly IViabilityEvaluator _evaluator;
    private readonly IPurchaseFlowBuilder _flowBuilder;
    private readonly ICreditCardRepository _creditCards;
    private readonly ICardPurchaseRepository _purchases;
    private readonly ICurrentUserService _currentUser;

    public GetMonthlyViabilityComparisonUseCase(
        IProjectionRunner runner,
        IViabilityEvaluator evaluator,
        IPurchaseFlowBuilder flowBuilder,
        ICreditCardRepository creditCards,
        ICardPurchaseRepository purchases,
        ICurrentUserService currentUser)
    {
        _runner = runner;
        _evaluator = evaluator;
        _flowBuilder = flowBuilder;
        _creditCards = creditCards;
        _purchases = purchases;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<MonthlyViabilityComparisonResponse>> ExecuteAsync(MonthlyViabilityComparisonRequest request)
    {
        var userId = _currentUser.UserId;
        var horizon = PlanningDefaults.NormalizeHorizon(request.HorizonMonths);
        var now = DateTime.UtcNow;

        CreditCard? card = null;
        var limitExceeded = false;

        if (request.CreditCardId is int cardId)
        {
            card = await _creditCards.GetByIdAsync(cardId, userId);
            if (card is null)
                return ResultEntity<MonthlyViabilityComparisonResponse>.Failure(MessageKeys.CreditCardNotFound);

            if (request.Installment)
            {
                var used = await _purchases.SumPendingInstallmentsByCardAsync(card.Id, userId);
                limitExceeded = used + request.Amount > card.CreditLimit;
            }
        }

        var baseContext = await _runner.RunAsync(userId, now, horizon, request.SafetyReserve);
        var reserve = baseContext.Parameters.SafetyReserve;

        var firstMonth = new DateTime(now.Year, now.Month, 1);
        var months = new List<MonthViabilityResponse>(horizon);

        for (var i = 0; i < horizon; i++)
        {
            var candidateMonth = firstMonth.AddMonths(i);

            var purchaseFlows = _flowBuilder.Build(
                card, request.Amount, candidateMonth, request.Installment, request.InstallmentsCount);

            var context = await _runner.RunAsync(userId, now, horizon, reserve, purchaseFlows);
            var viability = _evaluator.Evaluate(context.Projection, context.Parameters);
            if (limitExceeded)
                viability = ViabilityOverrides.WithCardLimitVeto(viability);

            months.Add(new MonthViabilityResponse(
                candidateMonth.Year, candidateMonth.Month, viability.Score, viability.Verdict.ToString()));
        }

        var response = new MonthlyViabilityComparisonResponse(months);
        return ResultEntity<MonthlyViabilityComparisonResponse>.Success(response, MessageKeys.OperationSuccess);
    }
}
