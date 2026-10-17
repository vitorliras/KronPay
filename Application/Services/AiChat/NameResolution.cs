namespace Application.Services.AiChat;

public sealed record NameResolution<T>(T? Match, IReadOnlyList<T> Candidates)
    where T : class
{
    public bool IsAmbiguous => Match is null && Candidates.Count > 1;
}
