namespace Application.DTOs.Goals;

public sealed record CategorySpendingGroupResponse(
    string Label,
    decimal Amount,
    int TransactionCount,
    bool IsRepeated);
