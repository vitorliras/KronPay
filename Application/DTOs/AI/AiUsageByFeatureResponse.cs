namespace Application.DTOs.AI;

public sealed record AiUsageByFeatureResponse(
    string Feature,
    int Calls,
    long InputTokens,
    long CachedInputTokens,
    long OutputTokens,
    decimal EstimatedCostUsd);
