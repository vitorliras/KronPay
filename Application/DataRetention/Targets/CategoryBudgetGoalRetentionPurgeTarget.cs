using Domain.Interfaces.Goals;

namespace Application.DataRetention.Targets;

public sealed class CategoryBudgetGoalRetentionPurgeTarget : IRetentionPurgeTarget
{
    private readonly ICategoryBudgetGoalRepository _repository;

    public CategoryBudgetGoalRetentionPurgeTarget(ICategoryBudgetGoalRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> PurgeAsync(DateTime cutoff)
    {
        var candidates = await _repository.GetDeactivatedOlderThanAsync(cutoff);
        if (candidates.Count == 0)
            return 0;

        await _repository.DeleteRangeAsync(candidates);
        return candidates.Count;
    }
}
