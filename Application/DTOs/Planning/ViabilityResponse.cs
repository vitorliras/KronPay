namespace Application.DTOs.Planning;

public sealed record ViabilityResponse(
    int Score,
    string Verdict,
    IReadOnlyList<ViabilityFindingResponse> Findings);
