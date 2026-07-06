using Domain.Entities.Goals;

namespace Domain.Interfaces.Goals;

public interface IFinancialGoalRepository
{
    Task<FinancialGoal?> GetByIdAsync(int id, int userId);
    Task<IEnumerable<FinancialGoal>> GetActiveAsync(int userId);
    Task<IEnumerable<FinancialGoal>> GetHistoryAsync(int userId, string? search);
    Task<bool> AddAsync(FinancialGoal goal);
    bool Update(FinancialGoal goal);
}
