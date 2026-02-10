namespace Application.DTOs.Transactions;

public sealed record TransactionFullResponse(
    int Id,
    int UserId,
    string Description,
    decimal Amount,
    string CodTypeTransaction,
    DateTime TransactionDate,
    string Status,
    int CategoryId,
    int? CategorItemyId,
    int? TransactionGroupId,
    DateTime CreatedAt
);

