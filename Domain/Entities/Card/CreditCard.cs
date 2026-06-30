using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.Card;

public sealed class CreditCard
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int BankId { get; private set; }
    public short DueDay { get; private set; }
    public short ClosingDay { get; private set; }
    public decimal CreditLimit { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool Active { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }

    protected CreditCard() { }

    public CreditCard(
        int id,
        int userId,
        int bankId,
        string description,
        short dueDay,
        short closingDay,
        decimal creditLimit)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException(MessageKeys.InvalidDescription);

        Id = id;
        Description = description.Trim();
        CreatedAt = DateTime.UtcNow;
        Active = true;
        DueDay = dueDay;
        ClosingDay = closingDay;
        BankId = bankId;
        CreditLimit = creditLimit;
        DeactivatedAt = null;
        UserId = userId;
    }

    public void Deactivate()
    {
        Active = false; 
        DeactivatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException(MessageKeys.InvalidDescription);

        Description = description.Trim();
    }

    public void SetBankAndDays(int bankId, short dueDay, short closingDay )
    {
        BankId = bankId;
        DueDay = dueDay;
        ClosingDay = closingDay;
    }
}
