using Domain.Models.Planning;

namespace Application.Planning.Flows;

/// <summary>
/// Fonte de fluxos financeiros: traduz uma origem real (transações, faturas, compromissos…)
/// em <see cref="FinancialFlow"/> normalizados dentro da janela [from, to].
/// Novas fontes plugam apenas registrando mais uma implementação (Open/Closed).
/// </summary>
public interface IFinancialFlowSource
{
    Task<IEnumerable<FinancialFlow>> GetFlowsAsync(int userId, DateTime from, DateTime to);
}
