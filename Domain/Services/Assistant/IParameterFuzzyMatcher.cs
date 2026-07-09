namespace Domain.Services.Assistant;

public interface IParameterFuzzyMatcher
{
    MatchResult? Match(string inputText, IReadOnlyCollection<MatchCandidate> candidates);
}
