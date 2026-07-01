using Domain.Models.Planning;

namespace Domain.Services.Planning;

public interface IViabilityEvaluator
{
    ViabilityResult Evaluate(FinancialProjection projection, ProjectionParameters parameters);
}
