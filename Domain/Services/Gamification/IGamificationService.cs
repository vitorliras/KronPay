using Domain.Enums.Gamification;

namespace Domain.Services.Gamification;

public interface IGamificationService
{
    Task TriggerInstantEvaluationAsync(int userId, MissionEventType type, int? relatedEntityId);
    Task<bool> HasUnlockedBadgeAsync(int userId, BadgeCode code);
    Task NotifyAssistantConversationAsync(int userId);
}
