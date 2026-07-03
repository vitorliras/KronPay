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

public sealed class SimulatePurchaseUseCase
    : IUseCase<SimulatePurchaseRequest, PurchaseSimulationResponse>
{
    private readonly IProjectionRunner _runner;
    private readonly IViabilityEvaluator _evaluator;
    private readonly IPurchaseFlowBuilder _flowBuilder;
    private readonly ICreditCardRepository _creditCards;
    private readonly ICardPurchaseRepository _purchases;
    private readonly ICurrentUserService _currentUser;

    public SimulatePurchaseUseCase(
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

    public async Task<ResultEntity<PurchaseSimulationResponse>> ExecuteAsync(SimulatePurchaseRequest request)
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
                return ResultEntity<PurchaseSimulationResponse>.Failure(MessageKeys.CreditCardNotFound);

            if (request.Installment)
            {
                var used = await _purchases.SumPendingInstallmentsByCardAsync(card.Id, userId);
                limitExceeded = used + request.Amount > card.CreditLimit;
            }
        }

        var purchaseFlows = _flowBuilder.Build(
            card, request.Amount, request.PurchaseDate, request.Installment, request.InstallmentsCount);

        var baseContext = await _runner.RunAsync(userId, now, horizon, request.SafetyReserve);
        var reserve = baseContext.Parameters.SafetyReserve;
        var simulatedContext = await _runner.RunAsync(userId, now, horizon, reserve, purchaseFlows);

        var viability = _evaluator.Evaluate(simulatedContext.Projection, simulatedContext.Parameters);
        if (limitExceeded)
            viability = ViabilityOverrides.WithCardLimitVeto(viability);

        var response = new PurchaseSimulationResponse(
            baseContext.Projection.FinalBalance,
            simulatedContext.Projection.FinalBalance,
            reserve,
            simulatedContext.Projection.FirstNegativeMonth?.Year,
            simulatedContext.Projection.FirstNegativeMonth?.Month,
            PlanningResponseMapper.Map(viability),
            PlanningResponseMapper.MapMonths(simulatedContext.Projection));

        return ResultEntity<PurchaseSimulationResponse>.Success(response, MessageKeys.OperationSuccess);
    }
}
