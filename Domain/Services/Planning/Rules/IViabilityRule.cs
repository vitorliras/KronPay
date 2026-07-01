using Domain.Models.Planning;

namespace Domain.Services.Planning.Rules;

/// <summary>
/// Critério plugável de viabilidade avaliado sobre a projeção. Cada regra devolve um
/// <see cref="RuleResult"/> (status, penalidade, veto, mensagem) — novas regras entram
/// sem alterar o motor nem o avaliador (Open/Closed, ADR 0012).
/// </summary>
public interface IViabilityRule
{
    RuleResult Evaluate(FinancialProjection projection, ProjectionParameters parameters);
}
