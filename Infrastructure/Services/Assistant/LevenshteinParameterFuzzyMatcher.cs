using System.Globalization;
using System.Text;
using Domain.Services.Assistant;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Assistant;

public sealed class LevenshteinParameterFuzzyMatcher : IParameterFuzzyMatcher
{
    private readonly double _similarityThreshold;
    private readonly double _minimumMargin;

    public LevenshteinParameterFuzzyMatcher(IConfiguration configuration)
    {
        _similarityThreshold = configuration.GetValue<double?>("Assistant:FuzzySimilarityThreshold") ?? 0.8;
        _minimumMargin = configuration.GetValue<double?>("Assistant:FuzzyMinimumMargin") ?? 0.1;
    }

    public MatchResult? Match(string inputText, IReadOnlyCollection<MatchCandidate> candidates)
    {
        if (string.IsNullOrWhiteSpace(inputText) || candidates.Count == 0)
            return null;

        var normalizedInput = Normalize(inputText);

        MatchResult? best = null;
        var secondBestSimilarity = double.MinValue;

        foreach (var candidate in candidates)
        {
            var candidateSimilarity = candidate.Terms
                .Select(term => Similarity(normalizedInput, Normalize(term)))
                .DefaultIfEmpty(double.MinValue)
                .Max();

            if (best is null || candidateSimilarity > best.Similarity)
            {
                secondBestSimilarity = best?.Similarity ?? double.MinValue;
                best = new MatchResult(candidate.Id, candidateSimilarity);
            }
            else if (candidateSimilarity > secondBestSimilarity)
            {
                secondBestSimilarity = candidateSimilarity;
            }
        }

        if (best is null || best.Similarity < _similarityThreshold)
            return null;

        return best.Similarity - secondBestSimilarity >= _minimumMargin ? best : null;
    }

    private static double Similarity(string a, string b)
    {
        var maxLength = Math.Max(a.Length, b.Length);
        if (maxLength == 0)
            return 1.0;

        return 1.0 - (double)LevenshteinDistance(a, b) / maxLength;
    }

    private static int LevenshteinDistance(string a, string b)
    {
        var distances = new int[a.Length + 1, b.Length + 1];

        for (var i = 0; i <= a.Length; i++)
            distances[i, 0] = i;

        for (var j = 0; j <= b.Length; j++)
            distances[0, j] = j;

        for (var i = 1; i <= a.Length; i++)
        {
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                distances[i, j] = Math.Min(
                    Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                    distances[i - 1, j - 1] + cost);
            }
        }

        return distances[a.Length, b.Length];
    }

    private static string Normalize(string text)
    {
        var normalized = text.Trim().ToLowerInvariant();
        var decomposed = normalized.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(character);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
