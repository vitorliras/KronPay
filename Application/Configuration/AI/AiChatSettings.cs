namespace Application.Configuration.AI;

public sealed class AiChatSettings
{
    public string? Model { get; set; }
    public decimal? InputPricePerMillion { get; set; }
    public decimal? CachedInputPricePerMillion { get; set; }
    public decimal? OutputPricePerMillion { get; set; }
    public int MaxMessagesPerDay { get; set; } = 100;
    public int MaxToolIterations { get; set; } = 6;
    public int MaxHistoryMessages { get; set; } = 10;
    public int MaxOutputTokens { get; set; } = 1200;
    public int MaxAttachmentsPerMessage { get; set; } = 3;
    public int MaxAttachmentsPerConversation { get; set; } = 5;
    public long MaxAttachmentBytes { get; set; } = 10 * 1024 * 1024;
    public int AttachmentTtlMinutes { get; set; } = 120;
}
