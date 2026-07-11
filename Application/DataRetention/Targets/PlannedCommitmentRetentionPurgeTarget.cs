using Domain.Interfaces.Planning;

namespace Application.DataRetention.Targets;

public sealed class PlannedCommitmentRetentionPurgeTarget : IRetentionPurgeTarget
{
    private readonly IPlannedCommitmentRepository _repository;

    public PlannedCommitmentRetentionPurgeTarget(IPlannedCommitmentRepository repository)
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
