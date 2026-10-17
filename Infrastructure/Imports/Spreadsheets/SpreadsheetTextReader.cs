using System.Globalization;
using System.Text;
using ExcelDataReader;

namespace Infrastructure.Imports.Spreadsheets;

public static class SpreadsheetTextReader
{
    static SpreadsheetTextReader()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public static IReadOnlyList<string> ReadRows(Stream stream)
    {
        var rows = new List<string>();

        using var reader = ExcelReaderFactory.CreateReader(stream);

        do
        {
            while (reader.Read())
            {
                var cells = new List<string>(reader.FieldCount);

                for (var column = 0; column < reader.FieldCount; column++)
                    cells.Add(FormatCell(reader.GetValue(column)));

                if (cells.All(string.IsNullOrWhiteSpace))
                    continue;

                rows.Add(string.Join(" | ", cells));
            }
        }
        while (reader.NextResult());

        return rows;
    }

    private static string FormatCell(object? value) => value switch
    {
        null => string.Empty,
        DateTime date => date.TimeOfDay == TimeSpan.Zero
            ? date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
        double number => number.ToString("0.############", CultureInfo.InvariantCulture),
        decimal number => number.ToString(CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString()?.Replace('\n', ' ').Replace('\r', ' ').Trim() ?? string.Empty
    };
}
