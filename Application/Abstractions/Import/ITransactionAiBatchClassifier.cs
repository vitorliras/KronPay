using Application.DTOs.Transactions;

public interface ITransactionAiBatchClassifier
{
    Task<IReadOnlyList<TransactionAiSuggestion>> SuggestBatchAsync(
        int userId,
        IReadOnlyList<ImportedTransactionResponse> transactions,
        bool useAi = false
    );
}