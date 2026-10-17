namespace Application.DTOs.AiChat;

public sealed record TransactionDraftResponse(
    string Description,
    decimal Amount,
    string Date,
    string Type,
    int? PaymentMethodId,
    int? CategoryId,
    int? CategoryItemId,
    string? ClassificationSource);
