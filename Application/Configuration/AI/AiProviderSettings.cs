namespace Application.Configuration.AI;

public sealed class AiProviderSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 60;
    public bool SupportsFiles { get; set; }
    public string? ToolReasoningEffort { get; set; }
    public bool StructuredChatOutput { get; set; }
    public decimal InputPricePerMillion { get; set; }
    public decimal CachedInputPricePerMillion { get; set; }
    public decimal OutputPricePerMillion { get; set; }
}
