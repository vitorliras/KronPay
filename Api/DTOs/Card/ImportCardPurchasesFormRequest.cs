namespace Api.DTOs.Card;

public sealed record ImportCardPurchasesFormRequest(
    IFormFile File,
    int CreditCardId,
    bool UseAi = false);
