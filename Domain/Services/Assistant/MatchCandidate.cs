namespace Domain.Services.Assistant;

public sealed record MatchCandidate(string Id, IReadOnlyCollection<string> Terms);
