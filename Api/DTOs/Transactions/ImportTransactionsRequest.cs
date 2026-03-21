namespace Api.DTOs.Transactions
{
    public sealed record ImportTransactionsFormRequest(
        IFormFile File,
        bool Preview = false,
        bool UseAi  = false
    );

}
