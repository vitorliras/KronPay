namespace Application.DTOs.AiChat;

public sealed record ChatAttachmentSummaryResponse(
    string Id,
    string FileName,
    int TransactionCount,
    string? PeriodStart,
    string? PeriodEnd,
    decimal TotalIncome,
    decimal TotalExpense,
    int CardInvoicePaymentCount,
    decimal CardInvoicePaymentTotal);
