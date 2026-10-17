namespace Application.DTOs.AI;

public sealed record AiStatusResponse(bool Enabled, string? Provider, string? Model, bool SupportsFiles);
