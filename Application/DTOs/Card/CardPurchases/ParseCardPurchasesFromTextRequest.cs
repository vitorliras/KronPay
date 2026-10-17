namespace Application.DTOs.Card.CardPurchases;

public sealed record ParseCardPurchasesFromTextRequest(int CreditCardId, string Text);
