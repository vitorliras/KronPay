using Domain.Enums.Gamification;
using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.Gamification;

public sealed class UserRankProfile
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int Score { get; private set; }
    public RankTier Tier { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    protected UserRankProfile() { }

    public UserRankProfile(int userId)
    {
        if (userId <= 0)
            throw new DomainException(MessageKeys.InvaldUser);

        UserId = userId;
        Score = 0;
        Tier = RankTier.Cobre;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApplyPointDelta(int delta)
    {
        Score = Math.Max(0, Score + delta);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTier(RankTier tier)
    {
        Tier = tier;
        UpdatedAt = DateTime.UtcNow;
    }
}
