using Microsoft.Extensions.AI;

namespace Infrastructure.AI.Chat;

public sealed class UsageTrackingChatClient : DelegatingChatClient
{
    public UsageTrackingChatClient(IChatClient innerClient)
        : base(innerClient)
    {
    }

    public int InputTokens { get; private set; }
    public int CachedInputTokens { get; private set; }
    public int OutputTokens { get; private set; }

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var response = await base.GetResponseAsync(messages, options, cancellationToken);

        InputTokens += (int)(response.Usage?.InputTokenCount ?? 0);
        CachedInputTokens += (int)(response.Usage?.CachedInputTokenCount ?? 0);
        OutputTokens += (int)(response.Usage?.OutputTokenCount ?? 0);

        return response;
    }

    protected override void Dispose(bool disposing)
    {
    }
}
