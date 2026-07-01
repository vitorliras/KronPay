using Domain.Models.Planning;

namespace Domain.Services.Planning;

public interface IFinancialProjectionService
{
    FinancialProjection Project(IEnumerable<FinancialFlow> flows, ProjectionParameters parameters);
}
