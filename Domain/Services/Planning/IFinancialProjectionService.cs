using Domain.Models.Planning;

namespace Domain.Services.Planning;

/// <summary>
/// Serviço de domínio (puro) que projeta o saldo mês a mês a partir de uma coleção
/// normalizada de <see cref="FinancialFlow"/>, sem conhecer as fontes reais.
/// </summary>
public interface IFinancialProjectionService
{
    FinancialProjection Project(IEnumerable<FinancialFlow> flows, ProjectionParameters parameters);
}
