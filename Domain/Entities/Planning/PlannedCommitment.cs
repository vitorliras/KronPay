using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.Planning;

/// <summary>
/// Compromisso financeiro previsto (receita ou despesa futura, recorrente ou pontual)
/// que ainda não é uma <c>Transaction</c>. É a origem dos fluxos de alta confiança da projeção.
/// </summary>
public sealed class PlannedCommitment
{
    /// <summary>Direção do fluxo: "I" = entrada (inflow), "O" = saída (outflow).</summary>
    private static readonly string[] ValidDirections = { "I", "O" };

    /// <summary>Periodicidade: "U" = único/pontual, "M" = mensal, "S" = semanal, "A" = anual.</summary>
    private static readonly string[] ValidPeriodicities = { "U", "M", "S", "A" };

    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public string Direction { get; private set; }
    public string Periodicity { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public int? CategoryId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool Active { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }

    protected PlannedCommitment() { }

    public PlannedCommitment(
        int userId,
        string description,
        decimal amount,
        string direction,
        string periodicity,
        DateTime startDate,
        DateTime? endDate = null,
        int? categoryId = null)
    {
        Validate(description, amount, direction, periodicity, startDate, endDate);

        UserId = userId;
        Description = description.Trim();
        Amount = amount;
        Direction = direction;
        Periodicity = periodicity;
        StartDate = startDate;
        EndDate = endDate;
        CategoryId = categoryId;
        CreatedAt = DateTime.UtcNow;
        Active = true;
        DeactivatedAt = null;
    }

    public void UpdateDetails(
        string description,
        decimal amount,
        string direction,
        string periodicity,
        DateTime startDate,
        DateTime? endDate,
        int? categoryId)
    {
        Validate(description, amount, direction, periodicity, startDate, endDate);

        Description = description.Trim();
        Amount = amount;
        Direction = direction;
        Periodicity = periodicity;
        StartDate = startDate;
        EndDate = endDate;
        CategoryId = categoryId;
    }

    public void Deactivate()
    {
        Active = false;
        DeactivatedAt = DateTime.UtcNow;
    }

    private static void Validate(
        string description,
        decimal amount,
        string direction,
        string periodicity,
        DateTime startDate,
        DateTime? endDate)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException(MessageKeys.InvalidDescription);

        if (amount <= 0)
            throw new DomainException(MessageKeys.InvalidAmount);

        if (!ValidDirections.Contains(direction))
            throw new DomainException(MessageKeys.InvalidDirection);

        if (!ValidPeriodicities.Contains(periodicity))
            throw new DomainException(MessageKeys.InvalidPeriodicity);

        if (endDate.HasValue && endDate.Value.Date < startDate.Date)
            throw new DomainException(MessageKeys.InvalidCommitmentPeriod);
    }
}
