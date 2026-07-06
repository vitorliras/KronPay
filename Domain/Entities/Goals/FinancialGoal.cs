using Domain.Enums.Goals;
using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.Goals;

public sealed class FinancialGoal
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Description { get; private set; }
    public decimal TargetAmount { get; private set; }
    public decimal CurrentAmount { get; private set; }
    public DateTime TargetDate { get; private set; }
    public GoalPriority Priority { get; private set; }
    public FinancialGoalStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int? PreviousAttemptGoalId { get; private set; }

    protected FinancialGoal() { }

    public FinancialGoal(
        int userId,
        string description,
        decimal targetAmount,
        DateTime targetDate,
        GoalPriority priority,
        int? previousAttemptGoalId = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException(MessageKeys.InvalidDescription);

        if (targetAmount <= 0)
            throw new DomainException(MessageKeys.InvalidGoalAmount);

        if (targetDate.Date <= DateTime.UtcNow.Date)
            throw new DomainException(MessageKeys.InvalidGoalDate);

        UserId = userId;
        Description = description.Trim();
        TargetAmount = targetAmount;
        CurrentAmount = 0;
        TargetDate = targetDate;
        Priority = priority;
        Status = FinancialGoalStatus.Active;
        CreatedAt = DateTime.UtcNow;
        PreviousAttemptGoalId = previousAttemptGoalId;
    }

    public bool IsPastDue(DateTime now) => Status == FinancialGoalStatus.Active && TargetDate.Date < now.Date;

    public void UpdateDetails(string description, decimal targetAmount, DateTime targetDate, GoalPriority priority)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException(MessageKeys.InvalidDescription);

        if (targetAmount <= 0)
            throw new DomainException(MessageKeys.InvalidGoalAmount);

        if (targetDate.Date <= DateTime.UtcNow.Date)
            throw new DomainException(MessageKeys.InvalidGoalDate);

        Description = description.Trim();
        TargetAmount = targetAmount;
        TargetDate = targetDate;
        Priority = priority;
    }

    public void RegisterContribution(decimal amount)
    {
        if (Status != FinancialGoalStatus.Active)
            throw new DomainException(MessageKeys.GoalNotActive);

        if (amount <= 0)
            throw new DomainException(MessageKeys.InvalidGoalAmount);

        CurrentAmount += amount;

        if (CurrentAmount >= TargetAmount)
            MarkAsCompleted();
    }

    public void MarkAsCompleted()
    {
        if (Status != FinancialGoalStatus.Active)
            throw new DomainException(MessageKeys.GoalNotActive);

        Status = FinancialGoalStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkAsExpired()
    {
        if (Status != FinancialGoalStatus.Active)
            throw new DomainException(MessageKeys.GoalNotActive);

        Status = FinancialGoalStatus.Expired;
    }
}
