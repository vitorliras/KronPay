using Domain.Models.Planning;

namespace Domain.Services.Planning;

public interface ISafetyReserveCalculator
{
    decimal Calculate(FinancialProjection projection);
}
