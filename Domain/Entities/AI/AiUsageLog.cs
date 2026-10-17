using Domain.Exceptions;
using Shared.Localization;

namespace Domain.Entities.AI;

public sealed class AiUsageLog
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Feature { get; private set; } = string.Empty;
    public string Provider { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int InputTokens { get; private set; }
    public int CachedInputTokens { get; private set; }
    public int OutputTokens { get; private set; }
    public decimal EstimatedCostUsd { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected AiUsageLog() { }

    public AiUsageLog(
        int userId,
        string feature,
        string provider,
        string model,
        int inputTokens,
        int cachedInputTokens,
        int outputTokens,
        decimal estimatedCostUsd)
    {
        if (userId <= 0)
            throw new DomainException(MessageKeys.InvaldUser);

        if (string.IsNullOrWhiteSpace(feature)
            || string.IsNullOrWhiteSpace(provider)
            || string.IsNullOrWhiteSpace(model))
            throw new DomainException(MessageKeys.InvalidDescription);

        if (inputTokens < 0
            || outputTokens < 0
            || cachedInputTokens < 0
            || cachedInputTokens > inputTokens
            || estimatedCostUsd < 0)
            throw new DomainException(MessageKeys.InvalidAmount);

        UserId = userId;
        Feature = feature.Trim();
        Provider = provider.Trim();
        Model = model.Trim();
        InputTokens = inputTokens;
        CachedInputTokens = cachedInputTokens;
        OutputTokens = outputTokens;
        EstimatedCostUsd = estimatedCostUsd;
        CreatedAt = DateTime.UtcNow;
    }
}
