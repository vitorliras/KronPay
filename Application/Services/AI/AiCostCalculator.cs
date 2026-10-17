using Application.Configuration.AI;

namespace Application.Services.AI;

public static class AiCostCalculator
{
    private const decimal TokensPerMillion = 1_000_000m;

    public static decimal Calculate(
        AiProviderSettings provider,
        int inputTokens,
        int cachedInputTokens,
        int outputTokens)
    {
        var uncachedInputTokens = Math.Max(inputTokens - cachedInputTokens, 0);

        return (uncachedInputTokens * provider.InputPricePerMillion
                + cachedInputTokens * provider.CachedInputPricePerMillion
                + outputTokens * provider.OutputPricePerMillion)
            / TokensPerMillion;
    }
}
