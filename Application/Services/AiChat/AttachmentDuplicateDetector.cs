using Application.Abstractions.AiChat;
using Domain.Services.Transactions;
using Shared.AI;
using TransactionEntity = Domain.Entities.Transactions.Transaction;

namespace Application.Services.AiChat;

public sealed class AttachmentDuplicateDetector
{
    private const double MinTokenOverlap = 0.5;

    public IReadOnlyList<string?> Detect(
        IReadOnlyList<ChatAttachment> attachments,
        IReadOnlyList<TransactionEntity> existing)
    {
        var flags = new List<string?>();
        var availableExisting = existing
            .Select(t => new Candidate(t.TransactionDate.Date, t.Amount, TransactionDescriptionNormalizer.Normalize(t.Description)))
            .ToList();

        var earlierRows = new List<Candidate>();

        foreach (var attachment in attachments)
        {
            var currentRows = new List<Candidate>();

            foreach (var row in attachment.Transactions)
            {
                var candidate = new Candidate(row.Date.Date, row.Amount, TransactionDescriptionNormalizer.Normalize(row.Description));

                flags.Add(Consume(availableExisting, candidate) ? DuplicateKinds.AlreadyRegistered
                    : Consume(earlierRows, candidate) ? DuplicateKinds.RepeatedInAttachment
                    : null);

                currentRows.Add(candidate);
            }

            earlierRows.AddRange(currentRows);
        }

        return flags;
    }

    private static bool Consume(List<Candidate> pool, Candidate row)
    {
        var index = pool.FindIndex(c => c.Date == row.Date && c.Amount == row.Amount && Similar(c.Description, row.Description));

        if (index < 0)
            return false;

        pool.RemoveAt(index);
        return true;
    }

    private static bool Similar(string a, string b)
    {
        if (a.Length == 0 || b.Length == 0)
            return a == b;

        if (a == b || a.Contains(b, StringComparison.Ordinal) || b.Contains(a, StringComparison.Ordinal))
            return true;

        var tokensA = a.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var tokensB = b.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var shared = tokensA.Intersect(tokensB).Count();

        return shared > 0 && (double)shared / Math.Min(tokensA.Count, tokensB.Count) >= MinTokenOverlap;
    }

    private sealed record Candidate(DateTime Date, decimal Amount, string Description);
}
