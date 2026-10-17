namespace Application.Configuration.AI;

public sealed class AiSettings
{
    public bool Enabled { get; set; }
    public string Provider { get; set; } = string.Empty;
    public Dictionary<string, AiProviderSettings> Providers { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
    public AiChatSettings Chat { get; set; } = new();
}
