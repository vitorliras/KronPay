
using Application.DTOs.Transactions;

namespace Application.Abstractions.Import
{
    public interface ITransactionAiClassifier
    {
        Task<TransactionAiSuggestion> SuggestAsync(
            int userId,
            string description,
            decimal amount,
            bool useAi = false
        );
    }
}
