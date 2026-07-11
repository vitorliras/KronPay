using Domain.Entities.Transactions;
using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.Card;

public sealed class CardInvoice
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int CreditCardId { get; private set; }
    public short ReferenceYear { get; private set; }
    public short ReferenceMonth { get; private set; }
    public DateTime ClosingDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Status { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public int? TransactionId { get; private set; }
    public Transaction? Transaction { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected CardInvoice() { }

    public CardInvoice(
        int userId,
        int creditCardId,
        short referenceYear,
        short referenceMonth,
        DateTime closingDate,
        DateTime dueDate)
    {
        UserId = userId;
        CreditCardId = creditCardId;
        ReferenceYear = referenceYear;
        ReferenceMonth = referenceMonth;
        ClosingDate = closingDate;
        DueDate = dueDate;
        TotalAmount = 0m;
        Status = "A";
        CreatedAt = DateTime.UtcNow;
    }

    public bool IsPaid => Status == "P";

    public void AddAmount(decimal value)
    {
        if (value <= 0)
            throw new DomainException(MessageKeys.InvalidAmount);

        TotalAmount += value;
    }

    public void SubtractAmount(decimal value)
    {
        if (value <= 0)
            throw new DomainException(MessageKeys.InvalidAmount);

        TotalAmount -= value;
    }

    public void Close() => Status = "F";

    public void Pay(Transaction transaction)
    {
        Status = "P";
        PaidAt = DateTime.UtcNow;
        Transaction = transaction;
    }

    public void Reopen()
    {
        Status = "A";
        PaidAt = null;
        TransactionId = null;
        Transaction = null;
    }
}
