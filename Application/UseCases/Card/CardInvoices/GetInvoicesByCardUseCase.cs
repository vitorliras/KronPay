using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Card.CardInvoices;
using Domain.Interfaces.Card;
using Shared.Localization;
using Shared.Results;
using System.Linq;

namespace Application.UseCases.Card.CardInvoices;

public sealed class GetInvoicesByCardUseCase
    : IUseCase<GetInvoicesByCardRequest, IEnumerable<CardInvoiceResponse>>
{
    private readonly ICardInvoiceRepository _invoiceRepository;
    private readonly ICurrentUserService _currentUser;

    public GetInvoicesByCardUseCase(
        ICardInvoiceRepository invoiceRepository,
        ICurrentUserService currentUser)
    {
        _invoiceRepository = invoiceRepository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<IEnumerable<CardInvoiceResponse>>> ExecuteAsync(GetInvoicesByCardRequest request)
    {
        var userId = _currentUser.UserId;

        var invoices = await _invoiceRepository.GetByCardAsync(request.CreditCardId, userId);

        var response = invoices.Select(x => new CardInvoiceResponse(
            x.Id,
            x.ReferenceYear,
            x.ReferenceMonth,
            x.ClosingDate,
            x.DueDate,
            x.TotalAmount,
            x.Status,
            x.PaidAt));

        return ResultEntity<IEnumerable<CardInvoiceResponse>>.Success(response, MessageKeys.OperationSuccess);
    }
}
