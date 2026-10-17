using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Card.CardPurchases;
using Application.Services.Card;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Card.CardPurchases;

public sealed class CreateCardPurchaseRangeUseCase
    : IUseCase<CreateCardPurchaseRangeRequest, CreateCardPurchaseRangeResponse>
{
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ICardPurchaseRepository _purchaseRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICategoryItemRepository _categoryItemRepository;
    private readonly CardPurchaseRegistrar _registrar;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateCardPurchaseRangeUseCase(
        ICreditCardRepository creditCardRepository,
        ICardPurchaseRepository purchaseRepository,
        ICategoryRepository categoryRepository,
        ICategoryItemRepository categoryItemRepository,
        CardPurchaseRegistrar registrar,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _creditCardRepository = creditCardRepository;
        _purchaseRepository = purchaseRepository;
        _categoryRepository = categoryRepository;
        _categoryItemRepository = categoryItemRepository;
        _registrar = registrar;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<CreateCardPurchaseRangeResponse>> ExecuteAsync(
        CreateCardPurchaseRangeRequest request)
    {
        var userId = _currentUser.UserId;

        var card = await _creditCardRepository.GetByIdAsync(request.CreditCardId, userId);
        if (card is null)
            return ResultEntity<CreateCardPurchaseRangeResponse>.Failure(MessageKeys.CreditCardNotFound);

        if (!await OwnsAllCategoriesAsync(userId, request.Purchases))
            return ResultEntity<CreateCardPurchaseRangeResponse>.Failure(MessageKeys.CategoryNotFound);

        var total = request.Purchases.Sum(p => p.TotalAmount);

        var usedLimit = await _purchaseRepository.SumPendingInstallmentsByCardAsync(card.Id, userId);
        if (usedLimit + total > card.CreditLimit)
            return ResultEntity<CreateCardPurchaseRangeResponse>.Failure(MessageKeys.CreditLimitExceeded);

        var session = _registrar.StartSession(card, userId);

        foreach (var item in request.Purchases)
        {
            var purchase = await session.RegisterAsync(
                item.Description,
                item.TotalAmount,
                item.PurchaseDate,
                item.InstallmentsCount,
                item.CategoryId,
                item.CategoryItemId);

            if (purchase is null)
                return ResultEntity<CreateCardPurchaseRangeResponse>.Failure(MessageKeys.InsertFalied);
        }

        if (!await _uow.CommitAsync())
            return ResultEntity<CreateCardPurchaseRangeResponse>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<CreateCardPurchaseRangeResponse>.Success(
            new CreateCardPurchaseRangeResponse(request.Purchases.Count, total),
            MessageKeys.OperationSuccess);
    }

    private async Task<bool> OwnsAllCategoriesAsync(int userId, IEnumerable<CreateCardPurchaseRangeItem> purchases)
    {
        var withCategory = purchases.Where(p => p.CategoryId is not null).ToList();

        if (withCategory.Count == 0)
            return true;

        var categoryIds = (await _categoryRepository.GetAllAsync(userId)).Select(c => c.Id).ToHashSet();

        if (withCategory.Any(p => !categoryIds.Contains(p.CategoryId!.Value)))
            return false;

        var withItem = withCategory.Where(p => p.CategoryItemId is not null).ToList();

        if (withItem.Count == 0)
            return true;

        var categoryByItem = (await _categoryItemRepository.GetAllByUserIdAsync(userId))
            .ToDictionary(i => i.Id, i => i.CategoryId);

        return withItem.All(p =>
            categoryByItem.TryGetValue(p.CategoryItemId!.Value, out var categoryId)
            && categoryId == p.CategoryId);
    }
}
