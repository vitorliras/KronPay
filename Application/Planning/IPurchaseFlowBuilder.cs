using Domain.Entities.Card;
using Domain.Models.Planning;

namespace Application.Planning;

/// <summary>
/// Converte uma compra simulada (à vista ou parcelada) em fluxos de saída. Quando há cartão,
/// as parcelas são posicionadas nas faturas via <c>ICreditCardBillingCalculator</c>; sem cartão,
/// caem mês a mês a partir da data da compra.
/// </summary>
public interface IPurchaseFlowBuilder
{
    IReadOnlyList<FinancialFlow> Build(
        CreditCard? card,
        decimal amount,
        DateTime purchaseDate,
        bool installment,
        short installmentsCount);
}
