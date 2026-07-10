using Domain.Enums.Gamification;
using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.Gamification;

public sealed class PointLedgerEntry
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public MissionEventType Type { get; private set; }
    public MissionEventSignificance Significance { get; private set; }
    public int PointsDelta { get; private set; }
    public RankTier TierAtEvent { get; private set; }
    public DateTime OccurredAt { get; private set; }

    protected PointLedgerEntry() { }

    public PointLedgerEntry(
        int userId,
        MissionEventType type,
        MissionEventSignificance significance,
        int pointsDelta,
        RankTier tierAtEvent)
    {
        if (userId <= 0)
            throw new DomainException(MessageKeys.InvaldUser);

        UserId = userId;
        Type = type;
        Significance = significance;
        PointsDelta = pointsDelta;
        TierAtEvent = tierAtEvent;
        OccurredAt = DateTime.UtcNow;
    }
}
