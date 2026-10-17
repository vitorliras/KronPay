using System.Text.Json;
using Application.Abstractions.AI;
using Application.Services.Transactions;
using Infrastructure.AI.Prompts;

namespace Infrastructure.AI.Transactions;

public sealed class LlmTransactionExtractionService : ITransactionExtractionService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IAiCompletionService _completionService;

    public LlmTransactionExtractionService(IAiCompletionService completionService)
    {
        _completionService = completionService;
    }

    public Task<IReadOnlyList<ExtractedTransaction>> ExtractFromTextAsync(
        int userId,
        string text,
        string feature,
        CancellationToken cancellationToken = default) =>
        ExtractAsync(userId, feature, TransactionExtractionPrompt.ForText(text, ExtractionSupport.TodayInSaoPaulo()), null, cancellationToken);

    public Task<IReadOnlyList<ExtractedTransaction>> ExtractFromTableAsync(
        int userId,
        string tableText,
        string feature,
        CancellationToken cancellationToken = default) =>
        ExtractAsync(userId, feature, TransactionExtractionPrompt.ForTable(tableText, ExtractionSupport.TodayInSaoPaulo()), null, cancellationToken);

    public Task<IReadOnlyList<ExtractedTransaction>> ExtractFromFileAsync(
        int userId,
        AiAttachment attachment,
        string feature,
        CancellationToken cancellationToken = default) =>
        ExtractAsync(userId, feature, TransactionExtractionPrompt.ForFile(ExtractionSupport.TodayInSaoPaulo()), new[] { attachment }, cancellationToken);

    private async Task<IReadOnlyList<ExtractedTransaction>> ExtractAsync(
        int userId,
        string feature,
        string prompt,
        IReadOnlyList<AiAttachment>? attachments,
        CancellationToken cancellationToken)
    {
        var completion = await _completionService.CompleteAsync(
            new AiCompletionRequest(userId, feature, TransactionExtractionPrompt.SystemPrompt, prompt, attachments),
            cancellationToken);

        return ExtractedTransactionValidator.Validate(ParseDrafts(completion.Text), ExtractionSupport.TodayInSaoPaulo());
    }

    private static IReadOnlyList<ExtractedTransactionDraft> ParseDrafts(string text)
    {
        var json = ExtractionSupport.ExtractJsonArray(text)
            ?? throw new InvalidOperationException("The AI response did not contain a JSON array.");

        return JsonSerializer.Deserialize<List<ExtractedTransactionDraft>>(json, JsonOptions)
            ?? new List<ExtractedTransactionDraft>();
    }
}
