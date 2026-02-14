using Application.Abstractions.Import;
using System.Globalization;

public sealed class CsvTransactionImportParser : ITransactionImportParser
{
    public bool CanParse(string fileName)
        => fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

    public async Task<IEnumerable<ImportedTransactionResponse>> ParseAsync(
        Stream fileStream,
        int userId)
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

            var date = DateTime.Parse(columns[0], CultureInfo.InvariantCulture);
            var description = columns[3];
            var amount = decimal.Parse(columns[4], CultureInfo.InvariantCulture);

            result.Add(new ImportedTransactionResponse(
                date,
                Math.Abs(amount),
                description,
                amount < 0 ? "E" : "I", 
                "P",
                0,
                null,
                null
            ));
        }

        return result;
    }
}
