using Domain.Exceptions;
using KronPay.Domain.Entities.Configuration;
using Shared.Localization;
using System.ComponentModel.DataAnnotations.Schema;

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

    public int? CategoryId { get; private set; }
    public int? CategoryItemId { get; private set; }
    public short? Installments { get; private set; }
    public int? IdPaymentMethod { get; private set; }

    public int? TransactionGroupId { get; private set; }
    public TransactionGroup? TransactionGroup { get; private set; }

    public DateTime CreatedAt { get; private set; }

    [NotMapped]
    public string? InstallmentsText
    {
        get
        {
            if (TransactionGroup == null)
                return null;

            var start = TransactionGroup.StartDate;
            var end = TransactionGroup.EndDate;

            var total =
                ((end.Value.Year - start.Year) * 12) +
                (end.Value.Month - start.Month) + 1;

            var atual =
                ((TransactionDate.Year - start.Year) * 12) +
                (TransactionDate.Month - start.Month) + 1;

            atual = Math.Clamp(atual, 1, total);

            return $"{atual}/{total}";
        }
    }

    protected Transaction() { }

    public Transaction(
        int userId,
        decimal amount,
        DateTime transactionDate,
        string description,
        string codTypeTransaction,
        int? categoryId = null,
        int? categoryItemId = null,
        short? installments = null,
        int? idPaymentMethod = null,
        TransactionGroup? transactionGroup = null,
        string? status = null
        )
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
        Installments = installments;
        IdPaymentMethod = idPaymentMethod;
        TransactionGroup = transactionGroup;
        CreatedAt = DateTime.UtcNow;

        if (string.IsNullOrEmpty(status))
        {
            if (CreatedAt.Date > transactionDate.Date)
                Status = "E";
            else
                Status = "O";

        }
        else
        {
            Status = status;
        }
    }

    public void Paid() => Status = "P";
    public void Cancel() => Status = "C";
    public void Open() => Status = "O";
    public void Expired() => Status = "E";
    public void SetUser(int userId) => UserId = userId;
    public void SetDate(DateTime date) => TransactionDate = date;

    public void UpdateAmount(decimal newAmount)
    {
        if (newAmount <= 0)
            throw new DomainException(MessageKeys.InvalidAmount);

        Amount = newAmount;
    }

    public void UpdateDescription(string description)
    {
        if (string.IsNullOrEmpty(description))
            throw new DomainException(MessageKeys.InvalidDescription);

        Description = description;
    }

    public void UpdateCategory(int? categoryId, int? categoryIdItem)
    {
        CategoryId = categoryId;
        CategoryItemId = categoryIdItem;
    }

    public void UpdateType(string type)
    {
        CodTypeTransaction = type;
    }
}
