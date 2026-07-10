using Application.Services.Gamification;
using Domain.Enums.Gamification;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Domain.Interfaces.Gamification;
using Domain.Services.Gamification;

namespace Application.Services.Gamification.Evaluators;

public sealed class CardMissionEvaluator : IMissionEvaluator
{
    private const decimal ConsciousUsageThreshold = 0.8m;
    private const decimal AtLimitThreshold = 0.9m;
    private const int SemesterStreakThreshold = 6;
    private const string HasCreditCardKey = "HasCreditCard";
    private const string CreditCardCountKey = "CreditCardCount";

    private readonly ICreditCardRepository _creditCards;
    private readonly ICardInvoiceRepository _cardInvoices;
    private readonly IConsistencyCounterRepository _counters;

    public CardMissionEvaluator(
        ICreditCardRepository creditCards,
        ICardInvoiceRepository cardInvoices,
        IConsistencyCounterRepository counters)
    {
        _creditCards = creditCards;
        _cardInvoices = cardInvoices;
        _counters = counters;
    }

    public async Task<IReadOnlyList<MissionEvaluationResult>> EvaluateAsync(int userId, DateTime asOf)
    {
        var results = new List<MissionEvaluationResult>();
        var cards = (await _creditCards.GetAllAsync(userId)).Where(c => c.Active).ToList();

        if (cards.Count > 0)
            await ConsistencyCounterUpdater.MarkOnceAsync(_counters, userId, HasCreditCardKey);

        await ConsistencyCounterUpdater.RecordValueAsync(_counters, userId, CreditCardCountKey, cards.Count);

        foreach (var card in cards)
        {
            var latestInvoice = (await _cardInvoices.GetByCardAsync(card.Id, userId))
                .OrderByDescending(i => i.ReferenceYear)
                .ThenByDescending(i => i.ReferenceMonth)
                .FirstOrDefault();

            var counterKey = $"CardOnTime:CreditCardId={card.Id}";

            if (latestInvoice is null)
            {
                var idleStreak = (await _counters.GetAsync(userId, counterKey))?.CurrentStreak ?? 0;

                results.Add(new MissionEvaluationResult(MissionEventType.CardInvoiceOnTime, card.Id, false));
                results.Add(new MissionEvaluationResult(MissionEventType.CardInvoiceLate, card.Id, false));
                results.Add(new MissionEvaluationResult(MissionEventType.CardInvoiceFullyPaid, card.Id, false));
                results.Add(new MissionEvaluationResult(MissionEventType.CardNoLateInvoiceSemester, card.Id, idleStreak >= SemesterStreakThreshold));
                results.Add(new MissionEvaluationResult(MissionEventType.CardConsciousLimitUsage, card.Id, false));
                results.Add(new MissionEvaluationResult(MissionEventType.CardAtLimit, card.Id, false));
                continue;
            }

            var isOverdueUnpaid = !latestInvoice.IsPaid && asOf.Date > latestInvoice.DueDate.Date;
            var isPaidOnTime = latestInvoice.IsPaid
                && latestInvoice.PaidAt.HasValue
                && latestInvoice.PaidAt.Value.Date <= latestInvoice.DueDate.Date;
            var isPaidLate = latestInvoice.IsPaid
                && latestInvoice.PaidAt.HasValue
                && latestInvoice.PaidAt.Value.Date > latestInvoice.DueDate.Date;
            var isLate = isOverdueUnpaid || isPaidLate;

            results.Add(new MissionEvaluationResult(MissionEventType.CardInvoiceOnTime, card.Id, isPaidOnTime));
            results.Add(new MissionEvaluationResult(MissionEventType.CardInvoiceLate, card.Id, isLate));
            results.Add(new MissionEvaluationResult(MissionEventType.CardInvoiceFullyPaid, card.Id, latestInvoice.IsPaid));

            var streak = (await _counters.GetAsync(userId, counterKey))?.CurrentStreak ?? 0;
            if (isPaidOnTime || isLate)
                streak = await ConsistencyCounterUpdater.UpdateStreakAsync(_counters, userId, counterKey, isPaidOnTime);

            results.Add(new MissionEvaluationResult(MissionEventType.CardNoLateInvoiceSemester, card.Id, streak >= SemesterStreakThreshold));

            var usageRatio = card.CreditLimit > 0 ? latestInvoice.TotalAmount / card.CreditLimit : 0m;
            results.Add(new MissionEvaluationResult(MissionEventType.CardConsciousLimitUsage, card.Id, usageRatio <= ConsciousUsageThreshold));
            results.Add(new MissionEvaluationResult(MissionEventType.CardAtLimit, card.Id, usageRatio >= AtLimitThreshold));
        }

        return results;
    }
}
