namespace Application.DTOs.Gamification;

public sealed record UserRankResponse(
    string Tier,
    string ProximityMessageKey,
    int BronzeBadgeCount,
    int PrataBadgeCount,
    int OuroBadgeCount);
