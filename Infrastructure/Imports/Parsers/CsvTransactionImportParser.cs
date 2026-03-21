using Application.Abstractions.Import;
using Domain.Entities.Configuration;
using System.Globalization;

public sealed class CsvTransactionImportParser : ITransactionImportParser
{
    public bool CanParse(string fileName)
        => fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

    public async Task<IEnumerable<ImportedTransactionResponse>> ParseAsync(
        Stream fileStream,
        int userId,
        IEnumerable<PaymentMethod>? paymentMethods = null,
        IEnumerable<Category>? categories = null)
    {
        var result = new List<ImportedTransactionResponse>();

        using var reader = new StreamReader(fileStream);
        var header = await reader.ReadLineAsync(); 

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var columns = line.Split(';');
            if (!string.IsNullOrEmpty(columns[0]))
            {
                var date = DateTime.Parse(columns[0]);
                var description = columns[3];
                var amount = decimal.Parse(columns[4]);
                var type = columns[1].ToUpper() ;

                if (type.Equals("DESPESA") || type.Equals("D"))
                    type = "E";
                if (type.Equals("RECEITA") || type.Equals("R"))
                    type = "I";
                if (type.Equals("INVESTMENTO") || type.Equals("V") || type.Equals("IVESTIMENT"))
                    type = "V";

                var idPaymentMethod = 1;
                if (paymentMethods.Any() && paymentMethods != null)
                {
                    var paymentMethod = paymentMethods.Where(x => x.Description.ToUpper().Normalize().Equals(columns[5].ToUpper())).FirstOrDefault();
                    if(paymentMethod != null)
                        idPaymentMethod = paymentMethod.Id;
                }

                var categoryId = 0;
                if (categories.Any() && categories != null)
                {
                    var category = categories.Where(x => x.Description.ToUpper().Normalize().Equals(columns[2].ToUpper())).FirstOrDefault();
                    if (category != null)
                        categoryId = category.Id;
                }

                result.Add(new ImportedTransactionResponse(
                    date,
                    Math.Abs(amount),
                    description,
                    type, 
                    "P",
                    idPaymentMethod,
                    categoryId == 0 ? null : categoryId,
                    null,
                    null,
                    null
                ));
            }
        }

        return result;
    }
}
