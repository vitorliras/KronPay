namespace Application.DTOs.Gamification;

public sealed record MissionCatalogResponse(
    string Type,
    string Area,
    string Significance,
    string MessageKey);
