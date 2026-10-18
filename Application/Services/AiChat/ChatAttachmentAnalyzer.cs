using Application.Abstractions.AiChat;
using Domain.Interfaces;
using Domain.Interfaces.Transactions;

namespace Application.Services.AiChat;

public sealed class ChatAttachmentAnalyzer
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly CardInvoicePaymentDetector _invoiceDetector;
    private readonly AttachmentDuplicateDetector _duplicateDetector;

    public ChatAttachmentAnalyzer(
        ITransactionRepository transactionRepository,
        ICreditCardRepository creditCardRepository,
        CardInvoicePaymentDetector invoiceDetector,
        AttachmentDuplicateDetector duplicateDetector)
    {
        _transactionRepository = transactionRepository;
        _creditCardRepository = creditCardRepository;
        _invoiceDetector = invoiceDetector;
        _duplicateDetector = duplicateDetector;
    }

    public async Task<IReadOnlyList<AnalyzedAttachmentRow>> AnalyzeAsync(
        int userId,
        IReadOnlyList<ChatAttachment> attachments)
    {
        var rows = attachments.SelectMany(a => a.Transactions).ToList();

        if (rows.Count == 0)
            return Array.Empty<AnalyzedAttachmentRow>();

        var from = rows.Min(r => r.Date).Date.AddDays(-1);
        var to = rows.Max(r => r.Date).Date.AddDays(1);

        var existing = (await _transactionRepository.GetByPeriodAsync(userId, from, to)).ToList();
        var cards = (await _creditCardRepository.GetAllAsync(userId)).ToList();
        var flags = _duplicateDetector.Detect(attachments, existing);

        var result = new List<AnalyzedAttachmentRow>(rows.Count);
        var position = 0;

        foreach (var attachment in attachments)
        {
            foreach (var row in attachment.Transactions)
            {
                var invoice = _invoiceDetector.Detect(row.Description, cards);

                result.Add(new AnalyzedAttachmentRow(
                    row,
                    attachment.Id,
                    attachment.FileName,
                    invoice.IsPayment,
                    invoice.CardId,
                    invoice.CardName,
                    flags[position++]));
            }
        }

        return result;
    }
}
