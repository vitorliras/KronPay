
using Domain.Entities.Configuration;

namespace Application.Abstractions.Import
{
    public interface ITransactionImportParser
    {
        bool CanParse(string fileName);

        Task<IEnumerable<ImportedTransactionResponse>> ParseAsync(
            Stream fileStream,
            int userId,
            IEnumerable<PaymentMethod>? paymentMethods = null,
            IEnumerable<Category>? categories = null
        );
    }
}
