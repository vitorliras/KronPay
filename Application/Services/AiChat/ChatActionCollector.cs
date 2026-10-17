using Application.DTOs.AiChat;

namespace Application.Services.AiChat;

public sealed class ChatActionCollector
{
    private readonly List<AiChatActionResponse> _actions = new();

    public IReadOnlyList<AiChatActionResponse> Actions => _actions;

    public void Add(AiChatActionResponse action) => _actions.Add(action);
}
