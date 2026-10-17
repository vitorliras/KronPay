using System.ComponentModel;

namespace Application.Abstractions.AiChat;

public sealed record TransactionToolItem(
    [property: Description("What the transaction was, for example \"almoco\"")] string? Description = null,
    [property: Description("Positive amount in BRL")] decimal? Amount = null,
    [property: Description("Date yyyy-MM-dd. Omit for today")] string? Date = null,
    [property: Description("expense, income, investment or redemption. Omit for expense")] string? Type = null,
    [property: Description("Payment method name if the user said it, for example pix")] string? PaymentMethodName = null,
    [property: Description("Category name if the user said it")] string? CategoryName = null);
