using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Card.CardPurchases;
using Domain.Entities.Card;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Domain.Services.Card;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Card.CardPurchases;

public sealed class CreateCardPurchaseUseCase
    : IUseCase<CreateCardPurchaseRequest, CardPurchaseResponse>
{
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ICardPurchaseRepository _purchaseRepository;
    private readonly ICardInvoiceRepository _invoiceRepository;
    private readonly ICreditCardBillingCalculator _billingCalculator;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateCardPurchaseUseCase(
        ICreditCardRepository creditCardRepository,
        ICardPurchaseRepository purchaseRepository,
        ICardInvoiceRepository invoiceRepository,
        ICreditCardBillingCalculator billingCalculator,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _creditCardRepository = creditCardRepository;
        _purchaseRepository = purchaseRepository;
        _invoiceRepository = invoiceRepository;
        _billingCalculator = billingCalculator;
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

        var purchase = new CardPurchase(
            userId,
            card.Id,
            request.Description,
            request.TotalAmount,
            request.PurchaseDate,
            request.InstallmentsCount,
            request.CategoryId,
            request.CategoryItemId);

        if (!await _purchaseRepository.AddAsync(purchase))
            return ResultEntity<CardPurchaseResponse>.Failure(MessageKeys.InsertFalied);

        var amounts = SplitInstallments(request.TotalAmount, request.InstallmentsCount);
        var installments = new List<CardInstallment>();

        for (short i = 0; i < request.InstallmentsCount; i++)
        {
            var referenceDate = request.PurchaseDate.AddMonths(i);
            var cycle = _billingCalculator.Resolve(card, referenceDate);

            var invoice = await _invoiceRepository.GetByCycleAsync(
                card.Id, userId, cycle.ReferenceYear, cycle.ReferenceMonth);

            if (invoice is null)
            {
                invoice = new CardInvoice(
                    userId,
                    card.Id,
                    cycle.ReferenceYear,
                    cycle.ReferenceMonth,
                    cycle.ClosingDate,
                    cycle.DueDate);

                if (!await _invoiceRepository.AddAsync(invoice))
                    return ResultEntity<CardPurchaseResponse>.Failure(MessageKeys.InsertFalied);
            }
            else if (invoice.IsPaid)
            {
                invoice.Reopen();
            }

            invoice.AddAmount(amounts[i]);

            installments.Add(new CardInstallment(
                userId,
                purchase,
                invoice,
                (short)(i + 1),
                amounts[i]));
        }

        if (!await _purchaseRepository.AddInstallmentsAsync(installments))
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

    private static decimal[] SplitInstallments(decimal total, short count)
    {
        var amounts = new decimal[count];
        var perInstallment = Math.Floor(total / count * 100m) / 100m;
        decimal accumulated = 0m;

        for (int i = 0; i < count - 1; i++)
        {
            amounts[i] = perInstallment;
            accumulated += perInstallment;
        }

        amounts[count - 1] = total - accumulated;
        return amounts;
    }
}
