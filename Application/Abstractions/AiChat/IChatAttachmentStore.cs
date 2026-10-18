namespace Application.Abstractions.AiChat;

public interface IChatAttachmentStore
{
    void Add(int userId, ChatAttachment attachment);

    ChatAttachment? Get(int userId, string id);

    IReadOnlyList<ChatAttachment> GetMany(int userId, IEnumerable<string> ids);
}
