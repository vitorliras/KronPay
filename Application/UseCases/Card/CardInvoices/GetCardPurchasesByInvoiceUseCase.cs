using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Card.CardInvoices;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Shared.Localization;
using Shared.Results;
using System.Linq;

namespace Application.UseCases.Card.CardInvoices;

public sealed class GetCardPurchasesByInvoiceUseCase
    : IUseCase<GetCardPurchasesByInvoiceRequest, IEnumerable<CardInstallmentResponse>>
{
    private readonly ICardInvoiceRepository _invoiceRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUser;

    public GetCardPurchasesByInvoiceUseCase(
        ICardInvoiceRepository invoiceRepository,
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUser)
    {
        _invoiceRepository = invoiceRepository;
        _categoryRepository = categoryRepository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<IEnumerable<CardInstallmentResponse>>> ExecuteAsync(GetCardPurchasesByInvoiceRequest request)
    {
        var userId = _currentUser.UserId;

        var installments = await _invoiceRepository.GetInstallmentsByInvoiceAsync(request.CardInvoiceId, userId);

        var categories = (await _categoryRepository.GetAllAsync(userId))
            .ToDictionary(c => c.Id, c => c.Description);

        var response = installments.Select(x => new CardInstallmentResponse(
            x.Id,
            x.CardPurchaseId,
            x.CardPurchase?.Description ?? string.Empty,
            x.InstallmentNumber,
            x.CardPurchase?.InstallmentsCount ?? (short)1,
            x.Amount,
            x.Status,
            x.CardPurchase?.CategoryId is int categoryId && categories.TryGetValue(categoryId, out var description)
                ? description
                : null,
            x.CardPurchase?.CategoryId,
            x.CardPurchase?.CategoryItemId));

        return ResultEntity<IEnumerable<CardInstallmentResponse>>.Success(response, MessageKeys.OperationSuccess);
    }
}
