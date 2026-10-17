using Application.Abstractions.AI;
using Application.Configuration.AI;
using Microsoft.Extensions.Options;

namespace Application.Services.AI;

public sealed class AiAvailability : IAiAvailability
{
    private const string KeylessProvider = "Ollama";

    private readonly IOptionsMonitor<AiSettings> _settings;

    public AiAvailability(IOptionsMonitor<AiSettings> settings)
    {
        _settings = settings;
    }

    public ActiveAiProvider? GetActive()
    {
        var settings = _settings.CurrentValue;

        if (!settings.Enabled || string.IsNullOrWhiteSpace(settings.Provider))
            return null;

        var entry = settings.Providers.FirstOrDefault(p =>
            string.Equals(p.Key, settings.Provider, StringComparison.OrdinalIgnoreCase));

        if (entry.Key is null)
            return null;

        var provider = entry.Value;

        if (string.IsNullOrWhiteSpace(provider.BaseUrl) || string.IsNullOrWhiteSpace(provider.Model))
            return null;

        var requiresKey = !string.Equals(entry.Key, KeylessProvider, StringComparison.OrdinalIgnoreCase);

        if (requiresKey && string.IsNullOrWhiteSpace(provider.ApiKey))
            return null;

        return new ActiveAiProvider(entry.Key, provider);
    }
}
