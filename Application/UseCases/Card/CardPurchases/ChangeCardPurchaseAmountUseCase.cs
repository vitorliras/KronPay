using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Card.CardPurchases;
using Domain.Entities.Card;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Shared.Localization;
using Shared.Results;
using System.Linq;

namespace Application.UseCases.Card.CardPurchases;

public sealed class ChangeCardPurchaseAmountUseCase
    : IUseCase<ChangeCardPurchaseAmountRequest, CardPurchaseResponse>
{
    private readonly ICardPurchaseRepository _purchaseRepository;
    private readonly ICardInvoiceRepository _invoiceRepository;
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ChangeCardPurchaseAmountUseCase(
        ICardPurchaseRepository purchaseRepository,
        ICardInvoiceRepository invoiceRepository,
        ICreditCardRepository creditCardRepository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _purchaseRepository = purchaseRepository;
        _invoiceRepository = invoiceRepository;
        _creditCardRepository = creditCardRepository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<CardPurchaseResponse>> ExecuteAsync(ChangeCardPurchaseAmountRequest request)
    {
        var userId = _currentUser.UserId;

        var purchase = await _purchaseRepository.GetByIdAsync(request.Id, userId);
        if (purchase is null || !purchase.Active)
            return ResultEntity<CardPurchaseResponse>.Failure(MessageKeys.ItemNotFound);

        var installments = (await _purchaseRepository.GetInstallmentsByPurchaseAsync(purchase.Id, userId))
            .Where(i => i.Status != "C")
            .ToList();

        if (installments.Count == 0 || installments.Any(i => i.Status != "P"))
            return ResultEntity<CardPurchaseResponse>.Failure(MessageKeys.CardPurchaseAmountLocked);

        var invoices = new Dictionary<int, CardInvoice>();
        foreach (var invoiceId in installments.Select(i => i.CardInvoiceId).Distinct())
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, userId);
            if (invoice is null)
                return ResultEntity<CardPurchaseResponse>.Failure(MessageKeys.ItemNotFound);

            if (invoice.IsPaid)
                return ResultEntity<CardPurchaseResponse>.Failure(MessageKeys.CardPurchaseAmountLocked);

            invoices[invoiceId] = invoice;
        }

        var card = await _creditCardRepository.GetByIdAsync(purchase.CreditCardId, userId);
        if (card is null)
            return ResultEntity<CardPurchaseResponse>.Failure(MessageKeys.CreditCardNotFound);

        var newTotal = request.InstallmentAmount * installments.Count;
        var usedByOtherPurchases =
            await _purchaseRepository.SumPendingInstallmentsByCardAsync(card.Id, userId)
            - installments.Sum(i => i.Amount);

        if (usedByOtherPurchases + newTotal > card.CreditLimit)
            return ResultEntity<CardPurchaseResponse>.Failure(MessageKeys.CreditLimitExceeded);

        foreach (var installment in installments)
        {
            var invoice = invoices[installment.CardInvoiceId];
            invoice.SubtractAmount(installment.Amount);
            invoice.AddAmount(request.InstallmentAmount);
            installment.UpdateAmount(request.InstallmentAmount);
        }

        purchase.UpdateTotalAmount(newTotal);

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
