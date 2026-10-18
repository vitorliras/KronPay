using Application.DTOs.AiChat;
using Application.DTOs.Transactions;
using Application.Services.Transactions;
using Domain.Entities.Configuration;

namespace Application.Services.AiChat;

public sealed class TransactionDraftCategorizer
{
    private readonly ImportedTransactionClassificationService _classification;

    public TransactionDraftCategorizer(ImportedTransactionClassificationService classification)
    {
        _classification = classification;
    }

    public async Task<IReadOnlyList<TransactionDraftResponse>> SuggestAsync(
        int userId,
        IReadOnlyList<TransactionDraftResponse> drafts,
        IReadOnlyList<Category> categories)
    {
        var pending = drafts
            .Select((d, i) => (Draft: d, Index: i))
            .Where(x => x.Draft.CategoryId is null)
            .ToList();

        if (pending.Count == 0)
            return drafts;

        var imported = pending
            .Select(x => new ImportedTransactionResponse(
                DateTime.Today, x.Draft.Amount, x.Draft.Description, x.Draft.Type, "P", 1, null, null, null, null))
            .ToList();

        var classified = await _classification.ClassifyAsync(userId, imported, categories, useAi: true);
        var result = drafts.ToList();

        for (var position = 0; position < pending.Count; position++)
        {
            var suggestion = classified[position];

            if (suggestion?.CategoryId is not { } categoryId
                || categories.FirstOrDefault(c => c.Id == categoryId)?.CodTypeTransaction != pending[position].Draft.Type)
                continue;

            result[pending[position].Index] = pending[position].Draft with
            {
                CategoryId = categoryId,
                CategoryItemId = suggestion.CategoryItemId,
                ClassificationSource = suggestion.ClassificationSource
            };
        }

        return result;
    }
}
