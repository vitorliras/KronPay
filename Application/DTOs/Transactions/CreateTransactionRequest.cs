namespace Application.DTOs.Transactions;

public sealed record CreateTransactionRequest(
    string Description,
    decimal Amount,
    DateTime TransactionDate,
    string CodTypeTransaction,
    string RecurrenceType,
    DateTime? EndDate,
    short Installments,
    int? CategoryId,
    int? CategoryItemId,
    int? idMethodPayment
);
