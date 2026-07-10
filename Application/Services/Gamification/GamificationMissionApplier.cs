using Application.Gamification;
using Domain.Entities.Gamification;
using Domain.Enums.Gamification;
using Domain.Interfaces.Gamification;
using Domain.Services.Gamification;

namespace Application.Services.Gamification;

public sealed class GamificationMissionApplier
{
    private readonly IUserRankProfileRepository _rankProfiles;
    private readonly IPointLedgerRepository _pointLedger;
    private readonly IMissionStateSnapshotRepository _snapshots;
    private readonly IRankScoringCalculator _scoringCalculator;
    private readonly IUserBadgeRepository _userBadges;
    private readonly BadgeUnlockEvaluator _badgeUnlockEvaluator;

    public GamificationMissionApplier(
        IUserRankProfileRepository rankProfiles,
        IPointLedgerRepository pointLedger,
        IMissionStateSnapshotRepository snapshots,
        IRankScoringCalculator scoringCalculator,
        IUserBadgeRepository userBadges,
        BadgeUnlockEvaluator badgeUnlockEvaluator)
    {
        _rankProfiles = rankProfiles;
        _pointLedger = pointLedger;
        _snapshots = snapshots;
        _scoringCalculator = scoringCalculator;
        _userBadges = userBadges;
        _badgeUnlockEvaluator = badgeUnlockEvaluator;
    }

    public async Task<bool> ApplyResultAsync(int userId, MissionEvaluationResult result)
    {
        var snapshot = await _snapshots.GetAsync(userId, result.Type, result.RelatedEntityId);
        var wasActive = snapshot?.IsConditionActive ?? false;
        var justTriggered = result.IsConditionActiveNow && !wasActive;

        if (justTriggered)
            await ApplyPointsAsync(userId, result.Type);

        if (snapshot is null)
        {
            snapshot = new MissionStateSnapshot(userId, result.Type, result.RelatedEntityId, result.IsConditionActiveNow);
            await _snapshots.AddAsync(snapshot);
        }
        else if (snapshot.IsConditionActive != result.IsConditionActiveNow)
        {
            snapshot.UpdateState(result.IsConditionActiveNow);
            _snapshots.Update(snapshot);
        }

        return justTriggered;
    }

    public async Task<IReadOnlyList<BadgeCode>> UnlockEligibleBadgesAsync(int userId)
    {
        var newBadges = await _badgeUnlockEvaluator.EvaluateAsync(userId);

        foreach (var badgeCode in newBadges)
            await _userBadges.AddAsync(new UserBadge(userId, badgeCode));

        return newBadges;
    }

    private async Task ApplyPointsAsync(int userId, MissionEventType type)
    {
        var catalogEntry = MissionCatalog.Get(type);

        var profile = await _rankProfiles.GetByUserIdAsync(userId);
        var isNewProfile = profile is null;

        if (isNewProfile)
        {
            profile = new UserRankProfile(userId);
            await _rankProfiles.AddAsync(profile);
        }

        var delta = _scoringCalculator.CalculatePointDelta(catalogEntry.Significance, profile!.Tier, catalogEntry.IsGain);
        profile.ApplyPointDelta(delta);

        var newTier = _scoringCalculator.CalculateTier(profile.Score);
        profile.UpdateTier(newTier);

        if (!isNewProfile)
            _rankProfiles.Update(profile);

        await _pointLedger.AddAsync(new PointLedgerEntry(userId, type, catalogEntry.Significance, delta, newTier));
    }
}
