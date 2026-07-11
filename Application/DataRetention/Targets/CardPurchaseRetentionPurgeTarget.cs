using Domain.Entities.Card;
using Domain.Interfaces.Card;

namespace Application.DataRetention.Targets;

public sealed class CardPurchaseRetentionPurgeTarget : IRetentionPurgeTarget
{
    private readonly ICardPurchaseRepository _repository;

    public CardPurchaseRetentionPurgeTarget(ICardPurchaseRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> PurgeAsync(DateTime cutoff)
    {
        var candidates = await _repository.GetDeactivatedOlderThanAsync(cutoff);
        if (candidates.Count == 0)
            return 0;

        var deletable = new List<CardPurchase>();

        foreach (var purchase in candidates)
        {
            var referenced = await _repository.ExistsInstallmentByCardPurchaseIdAsync(purchase.Id);
            if (!referenced)
                deletable.Add(purchase);
        }

        if (deletable.Count == 0)
            return 0;

        await _repository.DeleteRangeAsync(deletable);
        return deletable.Count;
    }
}
