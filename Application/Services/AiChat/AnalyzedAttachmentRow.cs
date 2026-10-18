using Application.Abstractions.AiChat;

namespace Application.Services.AiChat;

public sealed record AnalyzedAttachmentRow(
    AttachmentTransaction Transaction,
    string AttachmentId,
    string FileName,
    bool IsCardInvoicePayment,
    int? CardId,
    string? CardName,
    string? DuplicateKind);
