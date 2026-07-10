using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Card.CardInvoices;
using Application.Notifications;
using Domain.Entities.Transactions;
using Domain.Enums.Gamification;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Domain.Interfaces.Transactions;
using Domain.Services.Gamification;
using Microsoft.Extensions.Logging;
using Shared.Localization;
using Shared.Results;
using System.Linq;

namespace Application.UseCases.Card.CardInvoices;

public sealed class PayCardInvoiceUseCase
    : IUseCase<PayCardInvoiceRequest, CardInvoiceResponse>
{
    private readonly ICardInvoiceRepository _invoiceRepository;
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly INotificationService _notificationService;
    private readonly IGamificationService _gamificationService;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<PayCardInvoiceUseCase> _logger;

    public PayCardInvoiceUseCase(
        ICardInvoiceRepository invoiceRepository,
        ICreditCardRepository creditCardRepository,
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        INotificationService notificationService,
        IGamificationService gamificationService,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        ILogger<PayCardInvoiceUseCase> logger)
    {
        _invoiceRepository = invoiceRepository;
        _creditCardRepository = creditCardRepository;
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _notificationService = notificationService;
        _gamificationService = gamificationService;
        _uow = uow;
        _currentUser = currentUser;
        _logger = logger;
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

        // Paga apenas as parcelas PENDENTES (não recobra o que já foi quitado num pagamento anterior).
        var pending = (await _invoiceRepository.GetPendingInstallmentsByInvoiceAsync(invoice.Id, userId)).ToList();

        var amountToPay = pending.Sum(p => p.Amount);
        if (amountToPay <= 0)
            return ResultEntity<CardInvoiceResponse>.Failure(MessageKeys.CardInvoiceAlreadyPaid);

        var description = $"Fatura {card.Description} - {invoice.ReferenceMonth:00}/{invoice.ReferenceYear}";

        var invoiceCategory = await _categoryRepository.GetCardInvoiceCategoryAsync(userId);

        // Transação de saída (caixa) — reusa o domínio de Transaction.
        var transaction = new Transaction(
            userId,
            amountToPay,
            DateTime.UtcNow,
            description,
            request.CodTypeTransaction,
            invoiceCategory?.Id,
            null,
            null,
            request.PaymentMethodId,
            null,
            "P");

        if (!await _transactionRepository.AddAsync(transaction))
            return ResultEntity<CardInvoiceResponse>.Failure(MessageKeys.InsertFalied);

        foreach (var installment in pending)
            installment.Settle();

        // Liga a fatura à transação (EF resolve o FK no commit) e marca como paga.
        invoice.Pay(transaction);

        await _notificationService.ResolveByRelatedEntityAsync(userId, "CardInvoice", invoice.Id);

        if (!await _uow.CommitAsync())
            return ResultEntity<CardInvoiceResponse>.Failure(MessageKeys.DataPersistenceFailed);

        await TriggerGamificationBestEffort(userId, invoice.CreditCardId);

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

    private async Task TriggerGamificationBestEffort(int userId, int creditCardId)
    {
        try
        {
            await _gamificationService.TriggerInstantEvaluationAsync(userId, MissionEventType.CardInvoiceOnTime, creditCardId);

            if (!await _uow.CommitAsync())
                _logger.LogError(
                    "Falha ao persistir a avaliação instantânea de gamificação para o usuário {UserId}.", userId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha inesperada ao avaliar gamificação instantânea para o usuário {UserId}.", userId);
        }
    }
}
