using Domain.Enums.Gamification;

namespace Domain.Services.Gamification;

public interface IRankScoringCalculator
{
    int CalculatePointDelta(MissionEventSignificance significance, RankTier currentTier, bool isGain);
    RankTier CalculateTier(int score);
    int GetTierMinimumScore(RankTier tier);
    RankTier? GetNextTier(RankTier tier);
}
