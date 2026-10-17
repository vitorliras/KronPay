using System.ClientModel;
using System.Collections.Concurrent;
using Application.Abstractions.AI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace Infrastructure.AI;

public sealed class AiChatClientFactory : IAiChatClientFactory
{
    private const string KeylessPlaceholder = "not-required";

    private readonly ConcurrentDictionary<(string Name, string BaseUrl, string Model, string ApiKey, int Timeout), IChatClient> _clients = new();

    public IChatClient Get(ActiveAiProvider provider)
    {
        var settings = provider.Settings;

        var key = (provider.Name, settings.BaseUrl, settings.Model, settings.ApiKey, settings.TimeoutSeconds);

        return _clients.GetOrAdd(key, _ => Create(settings));
    }

    private static IChatClient Create(Application.Configuration.AI.AiProviderSettings settings)
    {
        var credential = new ApiKeyCredential(
            string.IsNullOrWhiteSpace(settings.ApiKey) ? KeylessPlaceholder : settings.ApiKey);

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(settings.BaseUrl),
            NetworkTimeout = TimeSpan.FromSeconds(settings.TimeoutSeconds)
        };

        return new OpenAIClient(credential, options)
            .GetChatClient(settings.Model)
            .AsIChatClient();
    }
}
