using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Card.CardInvoices;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Card.CardInvoices;

public sealed class GetCreditCardSummaryUseCase
    : IUseCase<GetCreditCardSummaryRequest, CreditCardSummaryResponse>
{
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ICardPurchaseRepository _purchaseRepository;
    private readonly ICurrentUserService _currentUser;

    public GetCreditCardSummaryUseCase(
        ICreditCardRepository creditCardRepository,
        ICardPurchaseRepository purchaseRepository,
        ICurrentUserService currentUser)
    {
        _creditCardRepository = creditCardRepository;
        _purchaseRepository = purchaseRepository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<CreditCardSummaryResponse>> ExecuteAsync(GetCreditCardSummaryRequest request)
    {
        var userId = _currentUser.UserId;

        var card = await _creditCardRepository.GetByIdAsync(request.CreditCardId, userId);
        if (card is null)
            return ResultEntity<CreditCardSummaryResponse>.Failure(MessageKeys.CreditCardNotFound);

        var used = await _purchaseRepository.SumPendingInstallmentsByCardAsync(card.Id, userId);
        var available = card.CreditLimit - used;

        return ResultEntity<CreditCardSummaryResponse>.Success(
            new CreditCardSummaryResponse(card.Id, card.CreditLimit, used, available),
            MessageKeys.OperationSuccess);
    }
}
