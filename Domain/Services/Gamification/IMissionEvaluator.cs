using Domain.Enums.Gamification;

namespace Domain.Services.Gamification;

public sealed record MissionEvaluationResult(MissionEventType Type, int? RelatedEntityId, bool IsConditionActiveNow);

public interface IMissionEvaluator
{
    Task<IReadOnlyList<MissionEvaluationResult>> EvaluateAsync(int userId, DateTime asOf);
}
