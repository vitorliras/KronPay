using Domain.Enums.Gamification;
using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.Gamification;

public sealed class MissionStateSnapshot
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public MissionEventType Type { get; private set; }
    public int? RelatedEntityId { get; private set; }
    public bool IsConditionActive { get; private set; }
    public DateTime LastEvaluatedAt { get; private set; }

    protected MissionStateSnapshot() { }

    public MissionStateSnapshot(
        int userId,
        MissionEventType type,
        int? relatedEntityId,
        bool isConditionActive)
    {
        if (userId <= 0)
            throw new DomainException(MessageKeys.InvaldUser);

        UserId = userId;
        Type = type;
        RelatedEntityId = relatedEntityId;
        IsConditionActive = isConditionActive;
        LastEvaluatedAt = DateTime.UtcNow;
    }

    public void UpdateState(bool isConditionActive)
    {
        IsConditionActive = isConditionActive;
        LastEvaluatedAt = DateTime.UtcNow;
    }
}
