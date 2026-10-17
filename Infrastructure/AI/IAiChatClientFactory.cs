using Application.Abstractions.AI;
using Microsoft.Extensions.AI;

namespace Infrastructure.AI;

public interface IAiChatClientFactory
{
    IChatClient Get(ActiveAiProvider provider);
}
