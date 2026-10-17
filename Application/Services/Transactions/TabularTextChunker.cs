namespace Application.Services.Transactions;

public static class TabularTextChunker
{
    public const int DefaultRowsPerChunk = 200;

    public static IReadOnlyList<string> Chunk(
        IReadOnlyList<string> rows,
        int rowsPerChunk = DefaultRowsPerChunk)
    {
        if (rows.Count == 0)
            return Array.Empty<string>();

        var header = rows[0];
        var body = rows.Skip(1).ToList();

        if (body.Count == 0)
            return new[] { header };

        return body
            .Chunk(rowsPerChunk)
            .Select(chunk => string.Join('\n', new[] { header }.Concat(chunk)))
            .ToList();
    }
}
