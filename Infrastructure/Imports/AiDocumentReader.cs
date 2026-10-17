using System.Text;
using Application.Abstractions.AI;
using Application.Services.Transactions;
using Domain.Exceptions;
using Infrastructure.Imports.Spreadsheets;
using Shared.Localization;

namespace Infrastructure.Imports;

public sealed class AiDocumentReader : IAiDocumentReader
{
    public const int MaxFileBytes = 10 * 1024 * 1024;
    public const int MaxRows = 3000;

    private readonly IAiAvailability _availability;

    public AiDocumentReader(IAiAvailability availability)
    {
        _availability = availability;
    }

    public async Task<AiDocument> ReadAsync(Stream stream)
    {
        var provider = _availability.GetActive()
            ?? throw new DomainException(MessageKeys.AiUnavailable);

        var bytes = await ReadBytesAsync(stream);

        return DetectKind(bytes) switch
        {
            FileKind.Pdf => FileDocument(provider, bytes, "application/pdf", "document.pdf"),
            FileKind.Png => FileDocument(provider, bytes, "image/png", "image.png"),
            FileKind.Jpeg => FileDocument(provider, bytes, "image/jpeg", "image.jpg"),
            FileKind.Spreadsheet => TableDocument(SpreadsheetTextReader.ReadRows(new MemoryStream(bytes))),
            _ => TableDocument(ReadTextRows(bytes))
        };
    }

    private static AiDocument FileDocument(ActiveAiProvider provider, byte[] bytes, string mediaType, string fileName)
    {
        if (!provider.Settings.SupportsFiles)
            throw new DomainException(MessageKeys.AiFilesUnsupported);

        return new AiDocument(new AiAttachment(bytes, mediaType, fileName), Array.Empty<string>());
    }

    private static AiDocument TableDocument(IReadOnlyList<string> rows)
    {
        if (rows.Count > MaxRows)
            throw new DomainException(MessageKeys.FileTooLarge);

        return new AiDocument(null, TabularTextChunker.Chunk(rows));
    }

    private static IReadOnlyList<string> ReadTextRows(byte[] bytes) =>
        Encoding.UTF8
            .GetString(bytes)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.TrimEnd('\r'))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

    private static async Task<byte[]> ReadBytesAsync(Stream stream)
    {
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);

        if (memory.Length > MaxFileBytes)
            throw new DomainException(MessageKeys.FileTooLarge);

        return memory.ToArray();
    }

    private static FileKind DetectKind(byte[] bytes)
    {
        if (StartsWith(bytes, 0x25, 0x50, 0x44, 0x46))
            return FileKind.Pdf;

        if (StartsWith(bytes, 0x89, 0x50, 0x4E, 0x47))
            return FileKind.Png;

        if (StartsWith(bytes, 0xFF, 0xD8, 0xFF))
            return FileKind.Jpeg;

        if (StartsWith(bytes, 0x50, 0x4B, 0x03, 0x04) || StartsWith(bytes, 0xD0, 0xCF, 0x11, 0xE0))
            return FileKind.Spreadsheet;

        return FileKind.Text;
    }

    private static bool StartsWith(byte[] bytes, params byte[] signature) =>
        bytes.Length >= signature.Length && bytes.AsSpan(0, signature.Length).SequenceEqual(signature);

    private enum FileKind
    {
        Pdf,
        Png,
        Jpeg,
        Spreadsheet,
        Text
    }
}
