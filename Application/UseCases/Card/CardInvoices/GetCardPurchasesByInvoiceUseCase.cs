using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Card.CardInvoices;
using Domain.Interfaces.Card;
using Shared.Localization;
using Shared.Results;
using System.Linq;

namespace Application.UseCases.Card.CardInvoices;

public sealed class GetCardPurchasesByInvoiceUseCase
    : IUseCase<GetCardPurchasesByInvoiceRequest, IEnumerable<CardInstallmentResponse>>
{
    private readonly ICardInvoiceRepository _invoiceRepository;
    private readonly ICurrentUserService _currentUser;

    public GetCardPurchasesByInvoiceUseCase(
        ICardInvoiceRepository invoiceRepository,
        ICurrentUserService currentUser)
    {
        _invoiceRepository = invoiceRepository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<IEnumerable<CardInstallmentResponse>>> ExecuteAsync(GetCardPurchasesByInvoiceRequest request)
    {
        var userId = _currentUser.UserId;

        var installments = await _invoiceRepository.GetInstallmentsByInvoiceAsync(request.CardInvoiceId, userId);

        var response = installments.Select(x => new CardInstallmentResponse(
            x.Id,
            x.CardPurchaseId,
            x.InstallmentNumber,
            x.Amount,
            x.Status));

        return ResultEntity<IEnumerable<CardInstallmentResponse>>.Success(response, MessageKeys.OperationSuccess);
    }
}
