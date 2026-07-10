using Domain.Entities.Gamification;

namespace Domain.Interfaces.Gamification;

public interface IGamificationEvaluationRunRepository
{
    Task<GamificationEvaluationRun?> GetAsync();
    Task<bool> AddAsync(GamificationEvaluationRun run);
    bool Update(GamificationEvaluationRun run);
}
