using Domain.Entities.Card;

namespace Domain.Services.Card;

public sealed class CreditCardBillingCalculator : ICreditCardBillingCalculator
{
    public BillingCycle Resolve(CreditCard card, DateTime purchaseDate)
    {
        int closingDay = card.ClosingDay;
        int dueDay = card.DueDay;

        var reference = new DateTime(purchaseDate.Year, purchaseDate.Month, 1);
        if (purchaseDate.Day > closingDay)
            reference = reference.AddMonths(1);

        var closingDate = DateAtDay(reference.Year, reference.Month, closingDay);

        var dueReference = dueDay <= closingDay ? reference.AddMonths(1) : reference;
        var dueDate = DateAtDay(dueReference.Year, dueReference.Month, dueDay);

        return new BillingCycle(
            (short)reference.Year,
            (short)reference.Month,
            closingDate,
            dueDate);
    }

    private static DateTime DateAtDay(int year, int month, int day)
    {
        var lastDay = DateTime.DaysInMonth(year, month);
        return new DateTime(year, month, Math.Min(day, lastDay));
    }
}
