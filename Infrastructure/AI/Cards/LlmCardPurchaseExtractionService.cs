using System.Text.Json;
using Application.Abstractions.AI;
using Application.Services.Card;
using Infrastructure.AI.Prompts;

namespace Infrastructure.AI.Cards;

public sealed class LlmCardPurchaseExtractionService : ICardPurchaseExtractionService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IAiCompletionService _completionService;

    public LlmCardPurchaseExtractionService(IAiCompletionService completionService)
    {
        _completionService = completionService;
    }

    public Task<IReadOnlyList<ExtractedCardPurchase>> ExtractFromTextAsync(
        int userId,
        string text,
        string feature,
        CancellationToken cancellationToken = default) =>
        ExtractAsync(
            userId,
            feature,
            CardPurchaseExtractionPrompt.ForText(text, ExtractionSupport.TodayInSaoPaulo()),
            null,
            singleInstallment: false,
            cancellationToken);

    public Task<IReadOnlyList<ExtractedCardPurchase>> ExtractFromTableAsync(
        int userId,
        string tableText,
        string feature,
        CancellationToken cancellationToken = default) =>
        ExtractAsync(
            userId,
            feature,
            CardPurchaseExtractionPrompt.ForTable(tableText, ExtractionSupport.TodayInSaoPaulo()),
            null,
            singleInstallment: true,
            cancellationToken);

    public Task<IReadOnlyList<ExtractedCardPurchase>> ExtractFromFileAsync(
        int userId,
        AiAttachment attachment,
        string feature,
        CancellationToken cancellationToken = default) =>
        ExtractAsync(
            userId,
            feature,
            CardPurchaseExtractionPrompt.ForFile(ExtractionSupport.TodayInSaoPaulo()),
            new[] { attachment },
            singleInstallment: true,
            cancellationToken);

    private async Task<IReadOnlyList<ExtractedCardPurchase>> ExtractAsync(
        int userId,
        string feature,
        string prompt,
        IReadOnlyList<AiAttachment>? attachments,
        bool singleInstallment,
        CancellationToken cancellationToken)
    {
        var completion = await _completionService.CompleteAsync(
            new AiCompletionRequest(userId, feature, CardPurchaseExtractionPrompt.SystemPrompt, prompt, attachments),
            cancellationToken);

        var validated = ExtractedCardPurchaseValidator.Validate(
            ParseDrafts(completion.Text),
            ExtractionSupport.TodayInSaoPaulo());

        return singleInstallment
            ? validated.Select(p => p with { InstallmentsCount = 1 }).ToList()
            : validated;
    }

    private static IReadOnlyList<ExtractedCardPurchaseDraft> ParseDrafts(string text)
    {
        var json = ExtractionSupport.ExtractJsonArray(text)
            ?? throw new InvalidOperationException("The AI response did not contain a JSON array.");

        return JsonSerializer.Deserialize<List<ExtractedCardPurchaseDraft>>(json, JsonOptions)
            ?? new List<ExtractedCardPurchaseDraft>();
    }
}
