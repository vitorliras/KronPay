using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.Transactions;

public sealed class TransactionGroup
{
    public int Id { get; private set; }
    public int UserId { get; private set; }

    public string Type { get; private set; }
    // FIXED = F | INFINITE = I

    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    public short? Installments { get; private set; }

    public bool Active { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected TransactionGroup() { }

    public TransactionGroup(
        int userId,
        string type,
        DateTime startDate,
        DateTime? endDate,
        short? installments)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new DomainException(MessageKeys.InvalidGroupType);

        UserId = userId;
        Type = type;
        StartDate = startDate;
        EndDate = endDate;
        Installments = installments;
        Active = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static TransactionGroup CreateFixed(
        int userId,
        DateTime startDate,
        short installments)
    {
        if (installments <= 1)
            throw new DomainException(MessageKeys.InvalidInstallments);

        return new TransactionGroup(
            userId,
            "F",
            startDate,
            startDate.AddMonths(installments - 1),
            installments
        );
    }

    public static TransactionGroup CreateInfinite(
        int userId,
        DateTime startDate)
    {
        return new TransactionGroup(
            userId,
            "I",
            startDate,
            null,
            null
        );
    }

    public void Cancel()
    {
        Active = false;
        EndDate = DateTime.UtcNow;
    }
}

