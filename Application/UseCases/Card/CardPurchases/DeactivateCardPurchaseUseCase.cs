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

public sealed class DeactivateCardPurchaseUseCase
    : IUseCase<DeactivateCardPurchaseRequest, CardPurchaseResponse>
{
    private readonly ICardPurchaseRepository _purchaseRepository;
    private readonly ICardInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeactivateCardPurchaseUseCase(
        ICardPurchaseRepository purchaseRepository,
        ICardInvoiceRepository invoiceRepository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _purchaseRepository = purchaseRepository;
        _invoiceRepository = invoiceRepository;
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

        // Carrega as faturas afetadas (rastreadas) e bloqueia se alguma já foi paga.
        var invoices = new Dictionary<int, CardInvoice>();
        foreach (var invoiceId in installments.Select(i => i.CardInvoiceId).Distinct())
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, userId);
            if (invoice is null)
                continue;

            if (invoice.IsPaid)
                return ResultEntity<CardPurchaseResponse>.Failure(MessageKeys.CardInvoiceAlreadyPaid);

            invoices[invoiceId] = invoice;
        }

        // Devolve o valor às faturas e cancela as parcelas (libera o limite).
        foreach (var installment in installments)
        {
            if (invoices.TryGetValue(installment.CardInvoiceId, out var invoice))
                invoice.SubtractAmount(installment.Amount);

            installment.Cancel();
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
