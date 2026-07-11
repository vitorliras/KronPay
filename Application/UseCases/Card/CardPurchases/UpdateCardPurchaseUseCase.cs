using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Card.CardPurchases;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Card.CardPurchases;

public sealed class UpdateCardPurchaseUseCase
    : IUseCase<UpdateCardPurchaseRequest, CardPurchaseResponse>
{
    private readonly ICardPurchaseRepository _purchaseRepository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateCardPurchaseUseCase(
        ICardPurchaseRepository purchaseRepository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _purchaseRepository = purchaseRepository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<CardPurchaseResponse>> ExecuteAsync(UpdateCardPurchaseRequest request)
    {
        var userId = _currentUser.UserId;

        var purchase = await _purchaseRepository.GetByIdAsync(request.Id, userId);
        if (purchase is null || !purchase.Active)
            return ResultEntity<CardPurchaseResponse>.Failure(MessageKeys.ItemNotFound);

        purchase.UpdateDescription(request.Description);
        purchase.UpdateCategory(request.CategoryId, request.CategoryItemId);

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
