namespace Application.Abstractions.AiChat;

public sealed record AttachmentTransaction(
    DateTime Date,
    string Description,
    decimal Amount,
    string Type);
