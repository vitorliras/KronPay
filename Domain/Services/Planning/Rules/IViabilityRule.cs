using Domain.Models.Planning;

namespace Domain.Services.Planning.Rules;

public interface IViabilityRule
{
    RuleResult Evaluate(FinancialProjection projection, ProjectionParameters parameters);
}
