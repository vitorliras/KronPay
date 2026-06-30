using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Card.CardInvoices;
using Domain.Interfaces.Card;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Card.CardInvoices;

public sealed class GetCardInvoiceUseCase
    : IUseCase<GetCardInvoiceRequest, CardInvoiceResponse>
{
    private readonly ICardInvoiceRepository _invoiceRepository;
    private readonly ICurrentUserService _currentUser;

    public GetCardInvoiceUseCase(
        ICardInvoiceRepository invoiceRepository,
        ICurrentUserService currentUser)
    {
        _invoiceRepository = invoiceRepository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<CardInvoiceResponse>> ExecuteAsync(GetCardInvoiceRequest request)
    {
        var userId = _currentUser.UserId;

        var invoice = await _invoiceRepository.GetByIdAsync(request.Id, userId);
        if (invoice is null)
            return ResultEntity<CardInvoiceResponse>.Failure(MessageKeys.ItemNotFound);

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
