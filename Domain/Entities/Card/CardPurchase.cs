using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.Card;

public sealed class CardPurchase
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int CreditCardId { get; private set; }
    public string Description { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime PurchaseDate { get; private set; }
    public short InstallmentsCount { get; private set; }
    public int? CategoryId { get; private set; }
    public int? CategoryItemId { get; private set; }
    public string Origin { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool Active { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }

    protected CardPurchase() { }

    public CardPurchase(
        int userId,
        int creditCardId,
        string description,
        decimal totalAmount,
        DateTime purchaseDate,
        short installmentsCount,
        int? categoryId = null,
        int? categoryItemId = null,
        string origin = "M")
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException(MessageKeys.InvalidDescription);

        if (totalAmount <= 0)
            throw new DomainException(MessageKeys.InvalidAmount);

        if (installmentsCount < 1)
            throw new DomainException(MessageKeys.InvalidInstallments);

        UserId = userId;
        CreditCardId = creditCardId;
        Description = description.Trim();
        TotalAmount = totalAmount;
        PurchaseDate = purchaseDate;
        InstallmentsCount = installmentsCount;
        CategoryId = categoryId;
        CategoryItemId = categoryItemId;
        Origin = origin;
        CreatedAt = DateTime.UtcNow;
        Active = true;
        DeactivatedAt = null;
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

    public void UpdateCategory(int? categoryId, int? categoryItemId)
    {
        CategoryId = categoryId;
        CategoryItemId = categoryItemId;
    }
}
