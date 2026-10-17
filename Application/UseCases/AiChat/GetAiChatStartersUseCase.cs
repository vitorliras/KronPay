using Application.Abstractions;
using Application.Abstractions.AI;
using Application.Abstractions.Common;
using Application.DTOs.AiChat;
using Application.Services.Spending;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.AiChat;

public sealed class GetAiChatStartersUseCase : IUseCaseWithoutRequest<AiChatStartersResponse>
{
    private const int MaxStarters = 4;
    private const int MinMonthsForRecurring = 3;

    private readonly IAiAvailability _availability;
    private readonly SpendingAggregator _aggregator;
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ICardInvoiceRepository _invoiceRepository;
    private readonly ICurrentUserService _currentUser;

    public GetAiChatStartersUseCase(
        IAiAvailability availability,
        SpendingAggregator aggregator,
        ICreditCardRepository creditCardRepository,
        ICardInvoiceRepository invoiceRepository,
        ICurrentUserService currentUser)
    {
        _availability = availability;
        _aggregator = aggregator;
        _creditCardRepository = creditCardRepository;
        _invoiceRepository = invoiceRepository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<AiChatStartersResponse>> ExecuteAsync()
    {
        if (_availability.GetActive() is null)
            return ResultEntity<AiChatStartersResponse>.Failure(MessageKeys.AiUnavailable);

        var userId = _currentUser.UserId;
        var today = BrazilClock.Today();
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var starters = new List<AiChatStarterResponse>();

        var top = (await _aggregator.GetCategoryTotalsAsync(userId, monthStart, today))
            .FirstOrDefault(c => c.CategoryId is not null);

        starters.Add(top is null
            ? new AiChatStarterResponse(MessageKeys.AiChatStarterSpendingMonth)
            : new AiChatStarterResponse(MessageKeys.AiChatStarterTopCategory, top.Category));

        if (await HasUnpaidInvoiceAsync(userId))
            starters.Add(new AiChatStarterResponse(MessageKeys.AiChatStarterNextInvoice));

        var months = await _aggregator.GetMonthlyTotalsAsync(userId, 6, today);

        if (months.Count(m => m.Expenses > 0) >= MinMonthsForRecurring)
            starters.Add(new AiChatStarterResponse(MessageKeys.AiChatStarterRecurring));

        starters.Add(new AiChatStarterResponse(MessageKeys.AiChatStarterAddTransaction));
        starters.Add(new AiChatStarterResponse(MessageKeys.AiChatStarterTour));

        return ResultEntity<AiChatStartersResponse>.Success(
            new AiChatStartersResponse(starters.Take(MaxStarters).ToList()),
            MessageKeys.OperationSuccess);
    }

    private async Task<bool> HasUnpaidInvoiceAsync(int userId)
    {
        foreach (var card in await _creditCardRepository.GetAllAsync(userId))
        {
            var pending = await _invoiceRepository.GetPendingAmountsByCardAsync(card.Id, userId);

            if ((await _invoiceRepository.GetByCardAsync(card.Id, userId)).Any(i => !i.IsPaid && pending.ContainsKey(i.Id)))
                return true;
        }

        return false;
    }
}
