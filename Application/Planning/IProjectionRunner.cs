using Domain.Models.Planning;

namespace Application.Planning;

/// <summary>
/// Orquestra a montagem de uma projeção: saldo inicial (realizadas pagas) + janela +
/// coleta de fluxos (agregador) + motor. Compartilhado pelos use cases de planejamento.
/// </summary>
public interface IProjectionRunner
{
    Task<ProjectionContext> RunAsync(
        int userId,
        DateTime referenceDate,
        int horizonMonths,
        decimal safetyReserve,
        IEnumerable<FinancialFlow>? extraFlows = null);
}
