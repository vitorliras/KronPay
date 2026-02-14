using Application.Abstractions.Import;
using Application.DTOs.Transactions;
using Infrastructure.AI.Transactions.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.AI.Transactions;

public sealed class TransactionAiClassifier : ITransactionAiBatchClassifier
{
    private readonly OllamaBatchTransactionClassifier _ollama;
    private readonly GroqBatchTransactionClassifier _groq;
    private readonly FallbackTransactionClassifier _fallback;
    private readonly IConfiguration _config;
    private readonly ILogger<TransactionAiClassifier> _logger;

    public TransactionAiClassifier(
        OllamaBatchTransactionClassifier ollama,
        GroqBatchTransactionClassifier groq,
        FallbackTransactionClassifier fallback,
        IConfiguration config,
        ILogger<TransactionAiClassifier> logger)
    {
        _ollama = ollama;
        _groq = groq;
        _fallback = fallback;
        _config = config;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TransactionAiSuggestion>> SuggestBatchAsync(
        int userId,
        IReadOnlyList<ImportedTransactionResponse> transactions,
        bool useAi = false)
    {
        if (transactions.Count == 0)
            return Array.Empty<TransactionAiSuggestion>();

        if (!useAi)
            return _fallback.ClassifyBatch(transactions);

        var provider = _config["AI:Provider"]?.ToLowerInvariant();

        try
        {
            return provider switch
            {
                "groq" => await _groq.SuggestBatchAsync(userId, transactions),
                "ollama" => await _ollama.SuggestBatchAsync(userId, transactions),
                _ => _fallback.ClassifyBatch(transactions)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI batch classification failed. Falling back.");
            return _fallback.ClassifyBatch(transactions);
        }
    }
}
