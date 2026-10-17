namespace Application.DTOs.Card.CardPurchases;

public sealed record ImportCardPurchasesRequest(
    Stream FileStream,
    string FileName,
    int CreditCardId,
    bool UseAi);
