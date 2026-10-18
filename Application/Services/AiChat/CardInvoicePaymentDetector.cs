using Domain.Entities.Card;
using Domain.Services.Transactions;

namespace Application.Services.AiChat;

public sealed class CardInvoicePaymentDetector
{
    private const int MinCardTokenLength = 4;

    private static readonly string[] PaymentTerms =
    {
        "pagamento fatura",
        "pagamento de fatura",
        "pagamento da fatura",
        "pagamento fatura cartao",
        "pgto fatura",
        "pgto de fatura",
        "pgto da fatura",
        "pagto fatura",
        "pagto de fatura",
        "pag fatura",
        "pag de fatura",
        "fatura cartao",
        "fatura do cartao",
        "fatura de cartao",
        "pagamento cartao de credito",
        "pgto cartao credito",
        "pagamento cartao credito"
    };

    private static readonly HashSet<string> GenericCardWords = new(StringComparer.Ordinal)
    {
        "cartao", "credito", "visa", "master", "mastercard", "black", "gold", "platinum", "card", "banco"
    };

    public CardInvoiceMatch Detect(string description, IReadOnlyList<CreditCard> cards)
    {
        var normalized = TransactionDescriptionNormalizer.Normalize(description);

        if (normalized.Length == 0)
            return CardInvoiceMatch.None;

        var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var mentionsInvoice = words.Contains("fatura");
        var matchesTerm = PaymentTerms.Any(term => normalized.Contains(term, StringComparison.Ordinal));

        var card = BestCard(words, cards);

        if (!matchesTerm && !(mentionsInvoice && card is not null))
            return CardInvoiceMatch.None;

        return new CardInvoiceMatch(true, card?.Id, card?.Description);
    }

    private static CreditCard? BestCard(string[] words, IReadOnlyList<CreditCard> cards)
    {
        CreditCard? best = null;
        var bestScore = 0;

        foreach (var card in cards)
        {
            var score = TransactionDescriptionNormalizer.Normalize(card.Description)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Count(token => token.Length >= MinCardTokenLength && !GenericCardWords.Contains(token) && words.Contains(token));

            if (score > bestScore)
            {
                best = card;
                bestScore = score;
            }
        }

        return best;
    }
}
