using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Card.CardPurchases;
using Application.Services.Card;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Card.CardPurchases;

public sealed class CreateCardPurchaseUseCase
    : IUseCase<CreateCardPurchaseRequest, CardPurchaseResponse>
{
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ICardPurchaseRepository _purchaseRepository;
    private readonly CardPurchaseRegistrar _registrar;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateCardPurchaseUseCase(
        ICreditCardRepository creditCardRepository,
        ICardPurchaseRepository purchaseRepository,
        CardPurchaseRegistrar registrar,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _creditCardRepository = creditCardRepository;
        _purchaseRepository = purchaseRepository;
        _registrar = registrar;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<CardPurchaseResponse>> ExecuteAsync(CreateCardPurchaseRequest request)
    {
        var userId = _currentUser.UserId;

        var card = await _creditCardRepository.GetByIdAsync(request.CreditCardId, userId);
        if (card is null)
            return ResultEntity<CardPurchaseResponse>.Failure(MessageKeys.CreditCardNotFound);

        var usedLimit = await _purchaseRepository.SumPendingInstallmentsByCardAsync(card.Id, userId);
        if (usedLimit + request.TotalAmount > card.CreditLimit)
            return ResultEntity<CardPurchaseResponse>.Failure(MessageKeys.CreditLimitExceeded);

        var purchase = await _registrar
            .StartSession(card, userId)
            .RegisterAsync(
                request.Description,
                request.TotalAmount,
                request.PurchaseDate,
                request.InstallmentsCount,
                request.CategoryId,
                request.CategoryItemId);

        if (purchase is null)
            return ResultEntity<CardPurchaseResponse>.Failure(MessageKeys.InsertFalied);

        if (!await _uow.CommitAsync())
            return ResultEntity<CardPurchaseResponse>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<CardPurchaseResponse>.Success(
            new CardPurchaseResponse(
                purchase.Id,
                purchase.Description,
                purchase.TotalAmount,
                purchase.InstallmentsCount),
            MessageKeys.OperationSuccess);
    }
}
