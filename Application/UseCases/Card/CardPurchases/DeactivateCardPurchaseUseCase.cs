using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Card.CardPurchases;
using Application.Services.Card;
using Domain.Entities.Card;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Shared.Localization;
using Shared.Results;
using System.Linq;

namespace Application.UseCases.Card.CardPurchases;

public sealed class DeactivateCardPurchaseUseCase
    : IUseCase<DeactivateCardPurchaseRequest, CardPurchaseResponse>
{
    private readonly ICardPurchaseRepository _purchaseRepository;
    private readonly ICardInvoiceRepository _invoiceRepository;
    private readonly CardInvoiceReconciler _reconciler;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeactivateCardPurchaseUseCase(
        ICardPurchaseRepository purchaseRepository,
        ICardInvoiceRepository invoiceRepository,
        CardInvoiceReconciler reconciler,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _purchaseRepository = purchaseRepository;
        _invoiceRepository = invoiceRepository;
        _reconciler = reconciler;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<CardPurchaseResponse>> ExecuteAsync(DeactivateCardPurchaseRequest request)
    {
        var userId = _currentUser.UserId;

        var purchase = await _purchaseRepository.GetByIdAsync(request.Id, userId);
        if (purchase is null || !purchase.Active)
            return ResultEntity<CardPurchaseResponse>.Failure(MessageKeys.ItemNotFound);

        var installments = (await _purchaseRepository.GetInstallmentsByPurchaseAsync(purchase.Id, userId))
            .Where(i => i.Status != "C")
            .ToList();

        var openInvoices = new Dictionary<int, CardInvoice>();
        foreach (var invoiceId in installments.Select(i => i.CardInvoiceId).Distinct())
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, userId);
            if (invoice is null || invoice.IsPaid)
                continue;

            openInvoices[invoiceId] = invoice;
        }

        foreach (var installment in installments)
        {
            if (!openInvoices.TryGetValue(installment.CardInvoiceId, out var invoice))
                continue;

            invoice.SubtractAmount(installment.Amount);
            installment.Cancel();
        }

        foreach (var (invoiceId, invoice) in openInvoices)
        {
            var cancelledIds = installments.Where(i => i.CardInvoiceId == invoiceId).Select(i => i.Id).ToList();
            await _reconciler.RestorePaidWhenFullySettledAsync(invoice, userId, cancelledIds);
        }

        purchase.Deactivate();

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
