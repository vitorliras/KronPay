using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.Card;

public sealed class CardInstallment
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int CardPurchaseId { get; private set; }
    public int CardInvoiceId { get; private set; }
    public CardPurchase? CardPurchase { get; private set; }
    public CardInvoice? CardInvoice { get; private set; }
    public short InstallmentNumber { get; private set; }
    public decimal Amount { get; private set; }
    public string Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected CardInstallment() { }

    public CardInstallment(
        int userId,
        CardPurchase purchase,
        CardInvoice invoice,
        short installmentNumber,
        decimal amount)
    {
        if (amount <= 0)
            throw new DomainException(MessageKeys.InvalidAmount);

        UserId = userId;
        CardPurchase = purchase;
        CardInvoice = invoice;
        InstallmentNumber = installmentNumber;
        Amount = amount;
        Status = "P";
        CreatedAt = DateTime.UtcNow;
    }

    public bool IsSettled => Status == "Q";

    public void Settle() => Status = "Q";

    public void Cancel() => Status = "C";
}
