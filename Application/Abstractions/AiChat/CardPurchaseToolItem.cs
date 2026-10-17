using System.ComponentModel;

namespace Application.Abstractions.AiChat;

public sealed record CardPurchaseToolItem(
    [property: Description("What was bought")] string? Description = null,
    [property: Description("TOTAL amount of the purchase in BRL")] decimal? TotalAmount = null,
    [property: Description("Purchase date yyyy-MM-dd. Omit for today")] string? Date = null,
    [property: Description("Number of installments. Omit for 1")] int? Installments = null,
    [property: Description("Category name if the user said it")] string? CategoryName = null);
