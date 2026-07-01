using Domain.Entities.Card;
using Domain.Enums.Planning;
using Domain.Models.Planning;
using Domain.Services.Card;

namespace Application.Planning;

public sealed class PurchaseFlowBuilder : IPurchaseFlowBuilder
{
    private readonly ICreditCardBillingCalculator _billingCalculator;

    public PurchaseFlowBuilder(ICreditCardBillingCalculator billingCalculator)
    {
        _billingCalculator = billingCalculator;
    }

    public IReadOnlyList<FinancialFlow> Build(
        CreditCard? card,
        decimal amount,
        DateTime purchaseDate,
        bool installment,
        short installmentsCount)
    {
        var flows = new List<FinancialFlow>();

        if (!installment || installmentsCount <= 1)
        {
            flows.Add(Flow(purchaseDate, amount));
            return flows;
        }

        var amounts = SplitInstallments(amount, installmentsCount);

        for (short i = 0; i < installmentsCount; i++)
        {
            var reference = purchaseDate.AddMonths(i);
            var competence = card is not null
                ? _billingCalculator.Resolve(card, reference).DueDate
                : reference;

            flows.Add(Flow(competence, amounts[i]));
        }

        return flows;
    }

    private static FinancialFlow Flow(DateTime date, decimal amount)
        => new(date, FlowDirection.Outflow, amount, ConfidenceLevel.High, FlowOrigin.Simulation, "Compra simulada");

    private static decimal[] SplitInstallments(decimal total, short count)
    {
        var amounts = new decimal[count];
        var perInstallment = Math.Floor(total / count * 100m) / 100m;
        decimal accumulated = 0m;

        for (var i = 0; i < count - 1; i++)
        {
            amounts[i] = perInstallment;
            accumulated += perInstallment;
        }

        amounts[count - 1] = total - accumulated;
        return amounts;
    }
}
