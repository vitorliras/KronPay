namespace Application.DTOs.Gamification;

public sealed record UserBadgeResponse(
    string Code,
    string Tier,
    string MessageKey,
    string DescriptionMessageKey,
    bool IsUnlocked,
    DateTime? UnlockedAt);
