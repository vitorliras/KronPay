using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.Transactions;

public sealed class Transaction
{
    public int Id { get; private set; }
    public int UserId { get; private set; }

    public decimal Amount { get; private set; }
    public DateTime TransactionDate { get; private set; }

    public string Description { get; private set; }
    public string CodTypeTransaction { get; private set; }
    public string Status { get; private set; }
    //(Open / Paid / Canceled)

    public int CategoryId { get; private set; }
    public int? CategoryItemId { get; private set; }

    public int? TransactionGroupId { get; private set; }
    public TransactionGroup? TransactionGroup { get; private set; } 

    public DateTime CreatedAt { get; private set; }

    protected Transaction() { }

    public Transaction(
        int userId,
        decimal amount,
        DateTime transactionDate,
        string description,
        string codTypeTransaction,
        int categoryId,
        int? categoryItemId = null,
        TransactionGroup? transactionGroup = null) 
    {
        if (amount <= 0)
            throw new DomainException(MessageKeys.InvalidAmount);

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException(MessageKeys.InvalidDescription);

        if (string.IsNullOrWhiteSpace(codTypeTransaction))
            throw new DomainException(MessageKeys.InvalidTypeTransaction);

        UserId = userId;
        Amount = amount;
        TransactionDate = transactionDate;
        Description = description.Trim();
        CodTypeTransaction = codTypeTransaction;

        CategoryId = categoryId;
        CategoryItemId = categoryItemId;

        TransactionGroup = transactionGroup; 

        Status = "O";
        CreatedAt = DateTime.UtcNow;
    }

    public void Paid() => Status = "P";
    public void Cancel() => Status = "C";
    public void Open() => Status = "O";

    public void VerifyAmount(decimal newAmount)
    {
        if (newAmount <= 0)
            throw new DomainException(MessageKeys.InvalidAmount);

        Amount = newAmount;
    }

    public void VerifyDescription(string description)
    {
        if (string.IsNullOrEmpty(description))
            throw new DomainException(MessageKeys.InvalidDescription);

        Description = description;
    }

    public void VerifyCategory(int categoryId, int? categoryIdItem)
    {
        if (categoryId <=0)
            throw new DomainException(MessageKeys.InvalidAmount);

        CategoryId = categoryId;
        CategoryItemId = categoryIdItem;
    }
}
