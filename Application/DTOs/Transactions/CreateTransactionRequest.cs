namespace Application.DTOs.Transactions;

public sealed record CreateTransactionRequest(
    int UserId,
    string Description,
    decimal Amount,
    DateTime TransactionDate,
    string CodTypeTransaction,   // E | I | V
    string RecurrenceType,       // F | I | N 
    DateTime? EndDate,          
    short Installments,
    int CategoryId,
    int? CategoryItemId
);
