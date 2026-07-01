using Domain.Models.Planning;

namespace Domain.Services.Planning;

/// <summary>Agrega as regras de viabilidade em veredito + score transparente.</summary>
public interface IViabilityEvaluator
{
    ViabilityResult Evaluate(FinancialProjection projection, ProjectionParameters parameters);
}
