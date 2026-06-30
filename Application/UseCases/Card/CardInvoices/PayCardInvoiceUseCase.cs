using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Card.CardInvoices;
using Domain.Entities.Transactions;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Domain.Interfaces.Transactions;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Card.CardInvoices;

public sealed class PayCardInvoiceUseCase
    : IUseCase<PayCardInvoiceRequest, CardInvoiceResponse>
{
    private readonly ICardInvoiceRepository _invoiceRepository;
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public PayCardInvoiceUseCase(
        ICardInvoiceRepository invoiceRepository,
        ICreditCardRepository creditCardRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _invoiceRepository = invoiceRepository;
        _creditCardRepository = creditCardRepository;
        _transactionRepository = transactionRepository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<CardInvoiceResponse>> ExecuteAsync(PayCardInvoiceRequest request)
    {
        var userId = _currentUser.UserId;

        var invoice = await _invoiceRepository.GetByIdAsync(request.CardInvoiceId, userId);
        if (invoice is null)
            return ResultEntity<CardInvoiceResponse>.Failure(MessageKeys.ItemNotFound);

        if (invoice.IsPaid)
            return ResultEntity<CardInvoiceResponse>.Failure(MessageKeys.CardInvoiceAlreadyPaid);

        var card = await _creditCardRepository.GetByIdAsync(invoice.CreditCardId, userId);
        if (card is null)
            return ResultEntity<CardInvoiceResponse>.Failure(MessageKeys.CreditCardNotFound);

        var description = $"Fatura {card.Description} - {invoice.ReferenceMonth:00}/{invoice.ReferenceYear}";

        // Transação de saída (caixa) — paga; reusa o domínio de Transaction.
        var transaction = new Transaction(
            userId,
            invoice.TotalAmount,
            DateTime.UtcNow,
            description,
            request.CodTypeTransaction,
            null,
            null,
            null,
            request.PaymentMethodId,
            null,
            "P");

        if (!await _transactionRepository.AddAsync(transaction))
            return ResultEntity<CardInvoiceResponse>.Failure(MessageKeys.InsertFalied);

        // Quita as parcelas pendentes da fatura (libera o limite no cálculo).
        var pending = await _invoiceRepository.GetPendingInstallmentsByInvoiceAsync(invoice.Id, userId);
        foreach (var installment in pending)
            installment.Settle();

        // Liga a fatura à transação (EF resolve o FK no commit) e marca como paga.
        invoice.Pay(transaction);

        if (!await _uow.CommitAsync())
            return ResultEntity<CardInvoiceResponse>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<CardInvoiceResponse>.Success(
            new CardInvoiceResponse(
                invoice.Id,
                invoice.ReferenceYear,
                invoice.ReferenceMonth,
                invoice.ClosingDate,
                invoice.DueDate,
                invoice.TotalAmount,
                invoice.Status,
                invoice.PaidAt),
            MessageKeys.OperationSuccess);
    }
}
