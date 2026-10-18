using Application.Abstractions;
using Application.Abstractions.AI;
using Application.Abstractions.AiChat;
using Application.Abstractions.Common;
using Application.Abstractions.Import;
using Application.DTOs.AiChat;
using Application.Services.AiChat;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.AiChat;

public sealed class UploadChatAttachmentsUseCase
    : IUseCase<UploadChatAttachmentsRequest, UploadChatAttachmentsResponse>
{
    private const int MaxRowsPerAttachment = 2000;

    private readonly IEnumerable<ITransactionImportParser> _parsers;
    private readonly IAiAvailability _availability;
    private readonly IChatAttachmentStore _store;
    private readonly ChatAttachmentToolService _tools;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UploadChatAttachmentsUseCase> _logger;

    public UploadChatAttachmentsUseCase(
        IEnumerable<ITransactionImportParser> parsers,
        IAiAvailability availability,
        IChatAttachmentStore store,
        ChatAttachmentToolService tools,
        ICurrentUserService currentUser,
        ILogger<UploadChatAttachmentsUseCase> logger)
    {
        _parsers = parsers;
        _availability = availability;
        _store = store;
        _tools = tools;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ResultEntity<UploadChatAttachmentsResponse>> ExecuteAsync(UploadChatAttachmentsRequest request)
    {
        var userId = _currentUser.UserId;
        var parsers = new List<ITransactionImportParser>();

        foreach (var file in request.Files)
        {
            var parser = _parsers.FirstOrDefault(p => p.CanParse(file.FileName));

            if (parser is null)
                return ResultEntity<UploadChatAttachmentsResponse>.Failure(MessageKeys.UnsupportedFileFormat);

            if (parser.IsAiBased && _availability.GetActive() is null)
                return ResultEntity<UploadChatAttachmentsResponse>.Failure(MessageKeys.AiUnavailable);

            parsers.Add(parser);
        }

        var ids = new List<string>();
        var unreadable = new List<string>();
        var lastFailure = MessageKeys.AiExtractionFailed;

        for (var index = 0; index < request.Files.Count; index++)
        {
            var file = request.Files[index];

            try
            {
                var parsed = await ParseWithFallbackAsync(parsers[index], file, userId);

                if (parsed.Count == 0)
                {
                    lastFailure = MessageKeys.AiNothingExtracted;
                    unreadable.Add(file.FileName);
                    continue;
                }

                var rows = parsed
                    .Take(MaxRowsPerAttachment)
                    .Select(t => new AttachmentTransaction(t.TransactionDate, t.Description, Math.Abs(t.Amount), t.Type))
                    .ToList();

                var attachment = new ChatAttachment(Guid.NewGuid().ToString("N"), file.FileName, rows, DateTime.UtcNow);

                _store.Add(userId, attachment);
                ids.Add(attachment.Id);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Chat attachment {FileName} could not be read: {Reason}.", file.FileName, ex.Message);
                lastFailure = ex.Message;
                unreadable.Add(file.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Chat attachment extraction failed for {FileName}.", file.FileName);
                lastFailure = MessageKeys.AiExtractionFailed;
                unreadable.Add(file.FileName);
            }
        }

        if (ids.Count == 0)
            return ResultEntity<UploadChatAttachmentsResponse>.Failure(lastFailure);

        var summaries = await _tools.DescribeAsync(userId, ids);

        return ResultEntity<UploadChatAttachmentsResponse>.Success(
            new UploadChatAttachmentsResponse(summaries, unreadable),
            MessageKeys.OperationSuccess);
    }

    private async Task<List<ImportedTransactionResponse>> ParseWithFallbackAsync(
        ITransactionImportParser parser,
        ChatUploadFile file,
        int userId)
    {
        var fallbackParser = IsCsv(file.FileName) && !parser.IsAiBased && _availability.GetActive() is not null
            ? _parsers.FirstOrDefault(p => p.IsAiBased)
            : null;

        var bytes = await ReadAllBytesAsync(file.Content);

        try
        {
            var parsed = (await parser.ParseAsync(new MemoryStream(bytes), userId)).ToList();

            if (parsed.Count > 0 || fallbackParser is null)
                return parsed;
        }
        catch (DomainException)
        {
            throw;
        }
        catch when (fallbackParser is null)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Tabular parser failed for a chat attachment. Falling back to the AI reader.");
        }

        return (await fallbackParser!.ParseAsync(new MemoryStream(bytes), userId)).ToList();
    }

    private static bool IsCsv(string fileName) =>
        fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream)
    {
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        return memory.ToArray();
    }
}
