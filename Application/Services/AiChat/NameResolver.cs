using Domain.Services.Assistant;
using Domain.Services.Transactions;

namespace Application.Services.AiChat;

public static class NameResolver
{
    public static NameResolution<T> Resolve<T>(
        string? input,
        IReadOnlyList<T> items,
        Func<T, int> idOf,
        Func<T, string> nameOf,
        IParameterFuzzyMatcher fuzzyMatcher)
        where T : class
    {
        var normalizedInput = TransactionDescriptionNormalizer.Normalize(input);

        if (normalizedInput.Length == 0 || items.Count == 0)
            return new NameResolution<T>(null, Array.Empty<T>());

        var exact = items.Where(i => TransactionDescriptionNormalizer.Normalize(nameOf(i)) == normalizedInput).ToList();

        if (exact.Count == 1)
            return new NameResolution<T>(exact[0], Array.Empty<T>());

        var contained = items
            .Where(i =>
            {
                var name = TransactionDescriptionNormalizer.Normalize(nameOf(i));
                return name.Contains(normalizedInput) || normalizedInput.Contains(name);
            })
            .ToList();

        if (contained.Count == 1)
            return new NameResolution<T>(contained[0], Array.Empty<T>());

        if (contained.Count > 1)
            return new NameResolution<T>(null, contained);

        var candidates = items
            .Select(i => new MatchCandidate(idOf(i).ToString(), new[] { nameOf(i) }))
            .ToList();

        var fuzzy = fuzzyMatcher.Match(input!, candidates);

        var match = fuzzy is null ? null : items.FirstOrDefault(i => idOf(i).ToString() == fuzzy.CandidateId);

        return new NameResolution<T>(match, Array.Empty<T>());
    }
}
