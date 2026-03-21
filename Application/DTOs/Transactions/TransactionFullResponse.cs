namespace Application.DTOs.Transactions;

public sealed record TransactionFullResponse(
    int Id,
    int UserId,
    string Description,
    decimal Amount,
    string CodTypeTransaction,
    DateTime TransactionDate,
    string Status,
    int?  IdPaymentMethod,
    int? CategoryId,
    int? Installments,
    int? categoryItemId,
    int? TransactionGroupId,
    string? InstallmentsText,
    string? TypeGroup,
    DateTime CreatedAt
);

