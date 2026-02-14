namespace Api.DTOs.Transactions
{
    public sealed record ImportTransactionsFormRequest(
        int UserId,
        IFormFile File,
        bool Preview = false,
        bool UseAi  = false
    );

}
