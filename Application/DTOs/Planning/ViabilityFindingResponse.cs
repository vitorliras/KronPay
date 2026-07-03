namespace Application.DTOs.Planning;

public sealed record ViabilityFindingResponse(
    string Rule,
    string Status,
    int Penalty,
    bool IsVeto,
    string MessageKey,
    int? Year,
    int? Month,
    IReadOnlyDictionary<string, string> Args);
