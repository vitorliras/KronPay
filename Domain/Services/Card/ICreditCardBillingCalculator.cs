using Domain.Entities.Card;

namespace Domain.Services.Card;

public interface ICreditCardBillingCalculator
{
    BillingCycle Resolve(CreditCard card, DateTime purchaseDate);
}

public sealed record BillingCycle(
    short ReferenceYear,
    short ReferenceMonth,
    DateTime ClosingDate,
    DateTime DueDate);
