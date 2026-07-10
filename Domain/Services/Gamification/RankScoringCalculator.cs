using Domain.Enums.Gamification;

namespace Domain.Services.Gamification;

public sealed class RankScoringCalculator : IRankScoringCalculator
{
    private static readonly List<RankTier> TierOrder = new List<RankTier>
    {
        RankTier.Cobre,
        RankTier.Bronze,
        RankTier.Prata,
        RankTier.Ouro,
        RankTier.Platina,
        RankTier.Diamante,
        RankTier.Supremo
    };

    private static readonly IReadOnlyDictionary<RankTier, int> TierMinimumScore =
        new Dictionary<RankTier, int>
        {
            [RankTier.Cobre] = 0,
            [RankTier.Bronze] = 100,
            [RankTier.Prata] = 250,
            [RankTier.Ouro] = 450,
            [RankTier.Platina] = 650,
            [RankTier.Diamante] = 800,
            [RankTier.Supremo] = 900
        };

    private static readonly IReadOnlyDictionary<RankTier, (int Pequeno, int Medio, int Grande)> GainTable =
        new Dictionary<RankTier, (int, int, int)>
        {
            [RankTier.Cobre] = (10, 25, 50),
            [RankTier.Bronze] = (8, 20, 40),
            [RankTier.Prata] = (6, 15, 30),
            [RankTier.Ouro] = (4, 10, 20),
            [RankTier.Platina] = (2, 5, 10),
            [RankTier.Diamante] = (2, 3, 5),
            [RankTier.Supremo] = (1, 2, 3)
        };

    private static readonly IReadOnlyDictionary<RankTier, (int Pequeno, int Medio, int Grande)> LossTable =
        new Dictionary<RankTier, (int, int, int)>
        {
            [RankTier.Cobre] = (5, 10, 20),
            [RankTier.Bronze] = (8, 15, 25),
            [RankTier.Prata] = (10, 20, 30),
            [RankTier.Ouro] = (15, 25, 35),
            [RankTier.Platina] = (18, 28, 38),
            [RankTier.Diamante] = (20, 30, 40),
            [RankTier.Supremo] = (22, 32, 45)
        };

    public int CalculatePointDelta(MissionEventSignificance significance, RankTier currentTier, bool isGain)
    {
        var values = (isGain ? GainTable : LossTable)[currentTier];

        var magnitude = significance switch
        {
            MissionEventSignificance.Pequeno => values.Pequeno,
            MissionEventSignificance.Medio => values.Medio,
            MissionEventSignificance.Grande => values.Grande,
            _ => values.Pequeno
        };

        return isGain ? magnitude : -magnitude;
    }

    public RankTier CalculateTier(int score)
    {
        var tier = RankTier.Cobre;

        foreach (var candidate in TierOrder)
        {
            if (score >= TierMinimumScore[candidate])
                tier = candidate;
        }

        return tier;
    }

    public int GetTierMinimumScore(RankTier tier) => TierMinimumScore[tier];

    public RankTier? GetNextTier(RankTier tier)
    {
        var index = TierOrder.IndexOf(tier);
        return index >= 0 && index < TierOrder.Count - 1 ? TierOrder[index + 1] : null;
    }
}
