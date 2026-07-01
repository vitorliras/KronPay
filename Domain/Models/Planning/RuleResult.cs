using Domain.Enums.Planning;

namespace Domain.Models.Planning;

public sealed record RuleResult(
    string RuleName,
    RuleStatus Status,
    int Penalty,
    bool IsVeto,
    string MessageKey,
    int? Year = null,
    int? Month = null)
{
    public static RuleResult Ok(string ruleName)
        => new(ruleName, RuleStatus.Ok, 0, false, string.Empty);
}
