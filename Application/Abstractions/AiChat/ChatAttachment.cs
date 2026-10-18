namespace Application.Abstractions.AiChat;

public sealed record ChatAttachment(
    string Id,
    string FileName,
    IReadOnlyList<AttachmentTransaction> Transactions,
    DateTime CreatedAtUtc);
