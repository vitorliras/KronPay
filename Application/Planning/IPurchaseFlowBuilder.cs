using Domain.Entities.Card;
using Domain.Models.Planning;

namespace Application.Planning;

public interface IPurchaseFlowBuilder
{
    IReadOnlyList<FinancialFlow> Build(
        CreditCard? card,
        decimal amount,
        DateTime purchaseDate,
        bool installment,
        short installmentsCount);
}
