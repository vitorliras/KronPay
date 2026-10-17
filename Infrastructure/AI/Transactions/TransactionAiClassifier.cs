using Application.Abstractions.AI;
using Application.DTOs.Transactions;
using Infrastructure.AI.Transactions.Tools;
using Microsoft.Extensions.Logging;

namespace Infrastructure.AI.Transactions;

public sealed class TransactionAiClassifier : ITransactionAiBatchClassifier
{
    private readonly LlmTransactionBatchClassifier _llm;
    private readonly FallbackTransactionClassifier _fallback;
    private readonly IAiAvailability _availability;
    private readonly ILogger<TransactionAiClassifier> _logger;

    public TransactionAiClassifier(
        LlmTransactionBatchClassifier llm,
        FallbackTransactionClassifier fallback,
        IAiAvailability availability,
        ILogger<TransactionAiClassifier> logger)
    {
        _llm = llm;
        _fallback = fallback;
        _availability = availability;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TransactionAiSuggestion>> SuggestBatchAsync(
        int userId,
        IReadOnlyList<ImportedTransactionResponse> transactions,
        bool useAi = false)
    {
        if (transactions.Count == 0)
            return Array.Empty<TransactionAiSuggestion>();

        if (!useAi || _availability.GetActive() is null)
            return _fallback.ClassifyBatch(transactions);

        try
        {
            return await _llm.SuggestBatchAsync(userId, transactions, useAi);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI batch classification failed. Falling back.");
            return _fallback.ClassifyBatch(transactions);
        }
    }
}
