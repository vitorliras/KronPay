namespace Application.Abstractions.AI;

public sealed record ExtractedTransaction(
    DateTime Date,
    string Description,
    decimal Amount,
    string Type)
{
    public ImportedTransactionResponse ToImportedTransaction() =>
        new(Date, Amount, Description, Type, "P", 1, null, null, null, null);
}
