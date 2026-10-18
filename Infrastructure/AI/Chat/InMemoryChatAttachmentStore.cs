using System.Collections.Concurrent;
using Application.Abstractions.AiChat;
using Application.Configuration.AI;
using Microsoft.Extensions.Options;

namespace Infrastructure.AI.Chat;

public sealed class InMemoryChatAttachmentStore : IChatAttachmentStore
{
    private readonly ConcurrentDictionary<int, List<ChatAttachment>> _byUser = new();
    private readonly IOptionsMonitor<AiSettings> _settings;

    public InMemoryChatAttachmentStore(IOptionsMonitor<AiSettings> settings)
    {
        _settings = settings;
    }

    public void Add(int userId, ChatAttachment attachment)
    {
        var chat = _settings.CurrentValue.Chat;
        var list = _byUser.GetOrAdd(userId, _ => new List<ChatAttachment>());

        lock (list)
        {
            PurgeExpired(list, chat.AttachmentTtlMinutes);
            list.Add(attachment);

            while (list.Count > Math.Max(1, chat.MaxAttachmentsPerConversation))
                list.RemoveAt(0);
        }
    }

    public ChatAttachment? Get(int userId, string id) =>
        GetMany(userId, new[] { id }).FirstOrDefault();

    public IReadOnlyList<ChatAttachment> GetMany(int userId, IEnumerable<string> ids)
    {
        if (!_byUser.TryGetValue(userId, out var list))
            return Array.Empty<ChatAttachment>();

        var wanted = ids.ToList();

        lock (list)
        {
            PurgeExpired(list, _settings.CurrentValue.Chat.AttachmentTtlMinutes);

            return wanted
                .Select(id => list.FirstOrDefault(a => a.Id == id))
                .OfType<ChatAttachment>()
                .ToList();
        }
    }

    private static void PurgeExpired(List<ChatAttachment> list, int ttlMinutes)
    {
        var limit = DateTime.UtcNow.AddMinutes(-Math.Max(1, ttlMinutes));
        list.RemoveAll(a => a.CreatedAtUtc < limit);
    }
}
