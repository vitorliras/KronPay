namespace Application.Services.AiChat;

public sealed class ChatAttachmentContext
{
    public IReadOnlyList<string> Ids { get; private set; } = Array.Empty<string>();

    public void Set(IEnumerable<string>? ids) =>
        Ids = ids?.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList() ?? new List<string>();
}
