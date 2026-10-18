namespace Application.Services.AiChat;

public sealed record CardInvoiceMatch(bool IsPayment, int? CardId, string? CardName)
{
    public static CardInvoiceMatch None { get; } = new(false, null, null);
}
