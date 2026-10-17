namespace Application.Abstractions.AI;

public sealed record AiCompletionResult(
    string Text,
    int InputTokens,
    int CachedInputTokens,
    int OutputTokens,
    decimal EstimatedCostUsd);
