using Application.DTOs.Assistant;
using Domain.Entities.Card;
using Domain.Interfaces;
using Shared.Localization;

namespace Application.Services.Assistant;

public sealed class CardAssistantResolver
{
    private readonly ICreditCardRepository _creditCardRepository;

    public CardAssistantResolver(ICreditCardRepository creditCardRepository)
    {
        _creditCardRepository = creditCardRepository;
    }

    public async Task<IReadOnlyList<CreditCard>> GetActiveCardsAsync(int userId) =>
        (await _creditCardRepository.GetAllAsync(userId)).Where(c => c.Active).ToList();

    public async Task<CreditCard?> GetByIdAsync(int cardId, int userId) =>
        await _creditCardRepository.GetByIdAsync(cardId, userId);

    public IReadOnlyList<AssistantOptionResponse> BuildSelectOptions(IReadOnlyList<CreditCard> cards) =>
        cards
            .Select(c => new AssistantOptionResponse(
                $"card:{c.Id}",
                MessageKeys.AssistantDynamicLabel,
                new[] { c.Description }))
            .ToList();
}
