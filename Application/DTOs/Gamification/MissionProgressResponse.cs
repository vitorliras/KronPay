namespace Application.DTOs.Gamification;

public sealed record MissionProgressResponse(
    string Type,
    string Area,
    string MessageKey,
    string Significance,
    bool IsGain,
    bool IsActive,
    DateTime LastEvaluatedAt);
