namespace Domain.Services.Assistant;

public interface IIntentEmbeddingMatcher
{
    MatchResult? Match(string inputText, IReadOnlyCollection<MatchCandidate> candidates);

    IReadOnlyList<MatchResult> Rank(string inputText, IReadOnlyCollection<MatchCandidate> candidates);
}
