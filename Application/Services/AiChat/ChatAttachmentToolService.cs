using System.Globalization;
using Application.Abstractions.AiChat;
using Application.Abstractions.Common;
using Application.DTOs.AiChat;
using Domain.Entities.Configuration;
using Domain.Interfaces;
using Domain.Services.Transactions;
using Shared.AI;

namespace Application.Services.AiChat;

public sealed class ChatAttachmentToolService
{
    private const int MaxListedRows = 30;
    private const int MaxModalRows = 500;
    private const int MaxDescriptionLength = 100;
    private const int TopDescriptions = 10;

    private readonly IChatAttachmentStore _store;
    private readonly ChatAttachmentContext _context;
    private readonly ChatAttachmentAnalyzer _analyzer;
    private readonly ICategoryRepository _categoryRepository;
    private readonly TransactionDraftCategorizer _categorizer;
    private readonly ChatActionCollector _actions;
    private readonly ICurrentUserService _currentUser;

    public ChatAttachmentToolService(
        IChatAttachmentStore store,
        ChatAttachmentContext context,
        ChatAttachmentAnalyzer analyzer,
        ICategoryRepository categoryRepository,
        TransactionDraftCategorizer categorizer,
        ChatActionCollector actions,
        ICurrentUserService currentUser)
    {
        _store = store;
        _context = context;
        _analyzer = analyzer;
        _categoryRepository = categoryRepository;
        _categorizer = categorizer;
        _actions = actions;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<ChatAttachmentSummaryResponse>> DescribeAsync(int userId, IEnumerable<string> ids)
    {
        var attachments = _store.GetMany(userId, ids);
        var rows = await _analyzer.AnalyzeAsync(userId, attachments);

        return attachments.Select(a => Describe(a, rows.Where(r => r.AttachmentId == a.Id).ToList())).ToList();
    }

    public async Task<ChatToolResult> SummarizeAsync(string? attachmentId)
    {
        var selected = Select(attachmentId, out var failure);
        if (failure is not null) return failure;

        var rows = await _analyzer.AnalyzeAsync(_currentUser.UserId, selected);

        var summaries = selected.Select(a =>
        {
            var own = rows.Where(r => r.AttachmentId == a.Id).ToList();
            var normal = own.Where(r => !r.IsCardInvoicePayment).ToList();

            return new
            {
                id = a.Id,
                file = a.FileName,
                summary = Describe(a, own),
                byMonth = normal
                    .GroupBy(r => new { r.Transaction.Date.Year, r.Transaction.Date.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => new
                    {
                        month = $"{g.Key.Year}-{g.Key.Month:00}",
                        expenses = Round(g.Where(r => IsExpense(r.Transaction.Type)).Sum(r => r.Transaction.Amount)),
                        income = Round(g.Where(r => IsIncome(r.Transaction.Type)).Sum(r => r.Transaction.Amount))
                    }),
                topExpenses = normal
                    .Where(r => IsExpense(r.Transaction.Type))
                    .GroupBy(r => TransactionDescriptionNormalizer.Normalize(r.Transaction.Description))
                    .Select(g => new { description = g.First().Transaction.Description, total = Round(g.Sum(r => r.Transaction.Amount)), times = g.Count() })
                    .OrderByDescending(x => x.total)
                    .Take(TopDescriptions),
                cardInvoicePayments = own
                    .Where(r => r.IsCardInvoicePayment)
                    .Select(r => new { date = Iso(r.Transaction.Date), card = r.CardName, amount = Round(r.Transaction.Amount) })
                    .Take(MaxListedRows)
            };
        }).ToList();

        return ChatToolResult.Success(
            new { attachments = summaries },
            "Expenses exclude credit card invoice payments (they are shown separately). Present both clearly.");
    }

    public async Task<ChatToolResult> GetTransactionsAsync(
        string? attachmentId, string? fromDate, string? toDate, string? text, string? type, string? kind, int limit)
    {
        var selected = Select(attachmentId, out var failure);
        if (failure is not null) return failure;

        var filter = BuildFilter(fromDate, toDate, text, null, type, out var filterError);
        if (filterError is not null) return filterError;

        var rows = (await _analyzer.AnalyzeAsync(_currentUser.UserId, selected))
            .Where(filter)
            .Where(r => kind switch
            {
                "card_invoice_payment" => r.IsCardInvoicePayment,
                "normal" => !r.IsCardInvoicePayment,
                _ => true
            })
            .OrderBy(r => r.Transaction.Date)
            .ToList();

        var shown = rows.Take(Math.Clamp(limit, 1, MaxListedRows)).Select(ToListedRow).ToList();

        return ChatToolResult.Success(new { totalMatches = rows.Count, shown = shown.Count, rows = shown });
    }

    public async Task<ChatToolResult> CheckDuplicatesAsync(string? attachmentId, string? fromDate, string? toDate)
    {
        var selected = Select(attachmentId, out var failure);
        if (failure is not null) return failure;

        var filter = BuildFilter(fromDate, toDate, null, null, null, out var filterError);
        if (filterError is not null) return filterError;

        var rows = (await _analyzer.AnalyzeAsync(_currentUser.UserId, selected)).Where(filter).ToList();
        var duplicates = rows.Where(r => r.DuplicateKind is not null).OrderBy(r => r.Transaction.Date).ToList();

        return ChatToolResult.Success(new
        {
            total = rows.Count,
            alreadyRegistered = duplicates.Count(r => r.DuplicateKind == DuplicateKinds.AlreadyRegistered),
            repeatedInAnotherAttachment = duplicates.Count(r => r.DuplicateKind == DuplicateKinds.RepeatedInAttachment),
            newRows = rows.Count - duplicates.Count,
            listed = duplicates.Take(MaxListedRows).Select(ToListedRow)
        });
    }

    public async Task<ChatToolResult> PrepareTransactionsAsync(
        string? attachmentId,
        string? fromDate,
        string? toDate,
        string? type,
        string? text,
        string? excludeText,
        bool includeDuplicates,
        bool includeCardInvoicePayments)
    {
        var userId = _currentUser.UserId;

        var selected = Select(attachmentId, out var failure);
        if (failure is not null) return failure;

        var filter = BuildFilter(fromDate, toDate, text, excludeText, type, out var filterError);
        if (filterError is not null) return filterError;

        var analyzed = (await _analyzer.AnalyzeAsync(userId, selected)).Where(filter).ToList();

        var rows = analyzed
            .Where(r => includeCardInvoicePayments || !r.IsCardInvoicePayment)
            .OrderBy(r => r.Transaction.Date)
            .ToList();

        if (rows.Count == 0)
            return ChatToolResult.Failure("No transactions match those filters in the attachment. Tell the user and ask what to change.");

        var categories = (await _categoryRepository.GetAllAsync(userId)).ToList();
        var invoiceCategory = await _categoryRepository.GetCardInvoiceCategoryAsync(userId);
        var truncated = rows.Count > MaxModalRows;

        var drafts = rows.Take(MaxModalRows).Select(r => new TransactionDraftResponse(
            Truncate(r.Transaction.Description),
            Math.Abs(r.Transaction.Amount),
            Iso(r.Transaction.Date),
            NormalizeType(r.Transaction.Type),
            null,
            r.IsCardInvoicePayment ? invoiceCategory?.Id : null,
            null,
            null,
            includeDuplicates ? null : r.DuplicateKind,
            r.IsCardInvoicePayment)).ToList();

        var suggested = await _categorizer.SuggestAsync(userId, drafts, categories);

        _actions.Add(new AiChatActionResponse(
            Shared.AI.AiChatActionTypes.OpenTransactionsModal,
            Path: "/transaction",
            TransactionDrafts: suggested));

        var duplicateCount = rows.Count(r => r.DuplicateKind is not null);
        var invoiceCount = rows.Count(r => r.IsCardInvoicePayment);

        return ChatToolResult.Success(
            new
            {
                inModal = suggested.Count,
                newRows = rows.Count - duplicateCount,
                alreadyRegistered = rows.Count(r => r.DuplicateKind == DuplicateKinds.AlreadyRegistered),
                repeatedInAnotherAttachment = rows.Count(r => r.DuplicateKind == DuplicateKinds.RepeatedInAttachment),
                duplicatesIncludedChecked = includeDuplicates,
                cardInvoicePayments = invoiceCount,
                cardInvoiceCategoryConfigured = invoiceCategory is not null,
                truncatedTo = truncated ? MaxModalRows : (int?)null
            },
            "The transaction form was opened for the user to review and save. NOTHING was saved yet. Report the counts: how many new, how many already registered or repeated (left unchecked unless includeDuplicates), and how many are credit card invoice payments (they entered as invoice payment, not as expenses, and do not mark the invoice as paid; to do that use the pay invoice flow). If cardInvoiceCategoryConfigured is false, tell the user to create the invoice category. Remind the user to choose the payment method in the form.");
    }

    private IReadOnlyList<ChatAttachment> Select(string? attachmentId, out ChatToolResult? failure)
    {
        failure = null;
        var all = _store.GetMany(_currentUser.UserId, _context.Ids);

        if (all.Count == 0)
        {
            failure = ChatToolResult.Failure("There is no attachment available in this conversation (it may have expired). Ask the user to attach the file again.");
            return all;
        }

        if (string.IsNullOrWhiteSpace(attachmentId))
            return all;

        var match = all.Where(a => a.Id == attachmentId
            || a.FileName.Contains(attachmentId, StringComparison.OrdinalIgnoreCase)).ToList();

        if (match.Count == 0)
            failure = ChatToolResult.Failure($"Attachment not found. Available: {string.Join(", ", all.Select(a => $"{a.FileName} (id {a.Id})"))}.");

        return match;
    }

    private static Func<AnalyzedAttachmentRow, bool> BuildFilter(
        string? fromDate, string? toDate, string? text, string? excludeText, string? type, out ChatToolResult? error)
    {
        error = null;
        DateTime? from = null;
        DateTime? to = null;

        if (!string.IsNullOrWhiteSpace(fromDate))
        {
            if (!TryDate(fromDate, out var parsed)) { error = ChatToolResult.Failure("fromDate must be yyyy-MM-dd."); return _ => true; }
            from = parsed;
        }

        if (!string.IsNullOrWhiteSpace(toDate))
        {
            if (!TryDate(toDate, out var parsed)) { error = ChatToolResult.Failure("toDate must be yyyy-MM-dd."); return _ => true; }
            to = parsed;
        }

        var wanted = TransactionDescriptionNormalizer.Normalize(text);
        var unwanted = TransactionDescriptionNormalizer.Normalize(excludeText);
        var kind = (type ?? string.Empty).Trim().ToLowerInvariant();

        return r =>
        {
            var normalized = TransactionDescriptionNormalizer.Normalize(r.Transaction.Description);

            return (from is null || r.Transaction.Date.Date >= from)
                && (to is null || r.Transaction.Date.Date <= to)
                && (wanted.Length == 0 || normalized.Contains(wanted, StringComparison.Ordinal))
                && (unwanted.Length == 0 || !normalized.Contains(unwanted, StringComparison.Ordinal))
                && (kind switch
                {
                    "expense" => IsExpense(r.Transaction.Type),
                    "income" => IsIncome(r.Transaction.Type),
                    _ => true
                });
        };
    }

    private static ChatAttachmentSummaryResponse Describe(ChatAttachment attachment, IReadOnlyList<AnalyzedAttachmentRow> rows)
    {
        var dates = attachment.Transactions.Select(t => t.Date.Date).ToList();
        var normal = rows.Where(r => !r.IsCardInvoicePayment).ToList();
        var invoices = rows.Where(r => r.IsCardInvoicePayment).ToList();

        return new ChatAttachmentSummaryResponse(
            attachment.Id,
            attachment.FileName,
            attachment.Transactions.Count,
            dates.Count == 0 ? null : Iso(dates.Min()),
            dates.Count == 0 ? null : Iso(dates.Max()),
            Round(normal.Where(r => IsIncome(r.Transaction.Type)).Sum(r => r.Transaction.Amount)),
            Round(normal.Where(r => IsExpense(r.Transaction.Type)).Sum(r => r.Transaction.Amount)),
            invoices.Count,
            Round(invoices.Sum(r => r.Transaction.Amount)));
    }

    private static object ToListedRow(AnalyzedAttachmentRow r) => new
    {
        date = Iso(r.Transaction.Date),
        description = r.Transaction.Description,
        amount = Round(r.Transaction.Amount),
        type = NormalizeType(r.Transaction.Type),
        kind = r.IsCardInvoicePayment ? "card_invoice_payment" : "normal",
        card = r.CardName,
        duplicate = r.DuplicateKind,
        file = r.FileName
    };

    private static bool IsExpense(string type) => type is "E" or "V";

    private static bool IsIncome(string type) => type is "I" or "R";

    private static string NormalizeType(string type) => type is "I" or "R" ? "I" : "E";

    private static string Truncate(string description)
    {
        var trimmed = description.Trim();
        return trimmed.Length <= MaxDescriptionLength ? trimmed : trimmed[..MaxDescriptionLength].TrimEnd();
    }

    private static bool TryDate(string? value, out DateTime date) =>
        DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    private static string Iso(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static decimal Round(decimal value) => Math.Round(value, 2);
}
