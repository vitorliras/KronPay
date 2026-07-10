using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Gamification;
using Application.Gamification;
using Domain.Interfaces.Gamification;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Gamification;

public sealed class GetActiveMissionsProgressUseCase : IUseCaseWithoutRequest<IReadOnlyList<MissionProgressResponse>>
{
    private readonly IMissionStateSnapshotRepository _snapshots;
    private readonly ICurrentUserService _currentUser;

    public GetActiveMissionsProgressUseCase(
        IMissionStateSnapshotRepository snapshots,
        ICurrentUserService currentUser)
    {
        _snapshots = snapshots;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<IReadOnlyList<MissionProgressResponse>>> ExecuteAsync()
    {
        var userId = _currentUser.UserId;

        var snapshots = await _snapshots.GetAllByUserAsync(userId);

        var response = snapshots
            .Select(ToResponse)
            .OrderByDescending(m => !m.IsGain && m.IsActive)
            .ThenByDescending(m => m.IsGain && m.IsActive)
            .ThenByDescending(m => m.LastEvaluatedAt)
            .ToList();

        return ResultEntity<IReadOnlyList<MissionProgressResponse>>.Success(response, MessageKeys.OperationSuccess);
    }

    private static MissionProgressResponse ToResponse(Domain.Entities.Gamification.MissionStateSnapshot snapshot)
    {
        var catalogEntry = MissionCatalog.Get(snapshot.Type);

        return new MissionProgressResponse(
            snapshot.Type.ToString(),
            catalogEntry.Area,
            catalogEntry.MessageKey,
            catalogEntry.Significance.ToString(),
            catalogEntry.IsGain,
            snapshot.IsConditionActive,
            snapshot.LastEvaluatedAt);
    }
}
