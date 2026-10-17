namespace Application.Abstractions.AI;

public sealed record ExtractedTransactionDraft(
    string? Date,
    string? Description,
    decimal? Amount,
    string? Type);
