namespace Application.DTOs.Card.CardPurchases;

public sealed record ChangeCardPurchaseAmountRequest(int Id, decimal InstallmentAmount);
