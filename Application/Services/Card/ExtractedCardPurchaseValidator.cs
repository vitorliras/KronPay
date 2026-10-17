using Application.Abstractions.AI;
using Application.Services.Transactions;

namespace Application.Services.Card;

public static class ExtractedCardPurchaseValidator
{
    public const int MaxInstallments = 48;
    public const int MaxDescriptionLength = 60;

    public static IReadOnlyList<ExtractedCardPurchase> Validate(
        IEnumerable<ExtractedCardPurchaseDraft> drafts,
        DateTime today)
    {
        var result = new List<ExtractedCardPurchase>();

        foreach (var draft in drafts)
        {
            if (!ExtractedTransactionValidator.TryNormalize(
                    draft.Date,
                    draft.Description,
                    draft.Amount,
                    today,
                    out var date,
                    out var description,
                    out var amount))
                continue;

            result.Add(new ExtractedCardPurchase(
                date,
                Truncate(description),
                amount,
                NormalizeInstallments(draft.Installments)));
        }

        return result;
    }

    private static string Truncate(string description) =>
        description.Length <= MaxDescriptionLength ? description : description[..MaxDescriptionLength].TrimEnd();

    private static short NormalizeInstallments(int? installments) =>
        installments is >= 1 and <= MaxInstallments ? (short)installments.Value : (short)1;
}
