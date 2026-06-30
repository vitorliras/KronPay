using Domain.Entities.Card;

namespace Domain.Services.Card;

/// <summary>
/// Serviço de domínio (puro) que resolve, a partir do cartão e da data da compra,
/// a qual ciclo/fatura a parcela pertence e as datas de fechamento e vencimento.
/// </summary>
public interface ICreditCardBillingCalculator
{
    BillingCycle Resolve(CreditCard card, DateTime purchaseDate);
}

public sealed record BillingCycle(
    short ReferenceYear,
    short ReferenceMonth,
    DateTime ClosingDate,
    DateTime DueDate);
