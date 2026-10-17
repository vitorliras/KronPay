namespace Application.DTOs.AI;

public sealed record AiUsageSummaryResponse(
    int Year,
    int Month,
    int Calls,
    long InputTokens,
    long CachedInputTokens,
    long OutputTokens,
    decimal EstimatedCostUsd,
    IReadOnlyList<AiUsageByFeatureResponse> ByFeature);
