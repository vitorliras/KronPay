
namespace Application.Abstractions.Import
{
    public interface ITransactionImportParser
    {
        bool CanParse(string fileName);

        Task<IEnumerable<ImportedTransactionResponse>> ParseAsync(
            Stream fileStream,
            int userId
        );
    }
}
