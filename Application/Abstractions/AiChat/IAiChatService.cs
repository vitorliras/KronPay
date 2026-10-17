namespace Application.Abstractions.AiChat;

public interface IAiChatService
{
    Task<AiChatServiceResult> RunAsync(
        int userId,
        string systemPrompt,
        IReadOnlyList<AiChatTurn> history,
        IChatToolbox toolbox,
        CancellationToken cancellationToken = default);
}
