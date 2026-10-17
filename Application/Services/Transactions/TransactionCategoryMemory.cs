using Domain.Services.Assistant;
using Domain.Services.Transactions;

namespace Application.Services.Transactions;

public sealed class TransactionCategoryMemory
{
    private const double ExactMinimumDominance = 0.6;
    private const double FuzzyMinimumDominance = 0.8;
    private const int FuzzyMinimumOccurrences = 2;
    private const int FuzzyMinimumLength = 5;

    private readonly IReadOnlyDictionary<(bool Outflow, string Description), TransactionCategoryMemoryEntry> _entries;
    private readonly IReadOnlyDictionary<bool, IReadOnlyCollection<MatchCandidate>> _candidates;
    private readonly IParameterFuzzyMatcher _fuzzyMatcher;

    public TransactionCategoryMemory(
        IReadOnlyDictionary<(bool Outflow, string Description), TransactionCategoryMemoryEntry> entries,
        IParameterFuzzyMatcher fuzzyMatcher)
    {
        _entries = entries;
        _fuzzyMatcher = fuzzyMatcher;

        _candidates = new Dictionary<bool, IReadOnlyCollection<MatchCandidate>>
        {
            [true] = BuildCandidates(entries, outflow: true),
            [false] = BuildCandidates(entries, outflow: false)
        };
    }

    public TransactionCategoryMemorySuggestion? Find(string description, string type)
    {
        var normalized = TransactionDescriptionNormalizer.Normalize(description);

        if (normalized.Length == 0)
            return null;

        var outflow = TransactionDescriptionNormalizer.IsOutflow(type);

        if (_entries.TryGetValue((outflow, normalized), out var exact))
            return exact.Dominance >= ExactMinimumDominance ? exact.Best : null;

        if (normalized.Length < FuzzyMinimumLength)
            return null;

        var match = _fuzzyMatcher.Match(normalized, _candidates[outflow]);

        if (match is null || !_entries.TryGetValue((outflow, match.CandidateId), out var approximate))
            return null;

        var accepted = approximate.TotalOccurrences >= FuzzyMinimumOccurrences
                       && approximate.Dominance >= FuzzyMinimumDominance;

        return accepted ? approximate.Best : null;
    }

    private static IReadOnlyCollection<MatchCandidate> BuildCandidates(
        IReadOnlyDictionary<(bool Outflow, string Description), TransactionCategoryMemoryEntry> entries,
        bool outflow) =>
        entries.Keys
            .Where(key => key.Outflow == outflow)
            .Select(key => new MatchCandidate(key.Description, new[] { key.Description }))
            .ToList();
}
