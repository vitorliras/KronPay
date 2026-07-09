using Application.DTOs.Assistant;
using Domain.Services.Assistant;
using Microsoft.Extensions.Configuration;

namespace Application.Services.Assistant;

public sealed class TopicDisambiguationResolver
{
    private const int MaxClarifyOptions = 3;

    private readonly IIntentEmbeddingMatcher _embeddingMatcher;
    private readonly double _similarityThreshold;
    private readonly double _minimumMargin;

    public TopicDisambiguationResolver(IIntentEmbeddingMatcher embeddingMatcher, IConfiguration configuration)
    {
        _embeddingMatcher = embeddingMatcher;
        _similarityThreshold = configuration.GetValue<double?>("Assistant:SimilarityThreshold") ?? 0.75;
        _minimumMargin = configuration.GetValue<double?>("Assistant:TopicMinimumMargin") ?? 0.025;
    }

    public TopicResolution Resolve(string freeText, IReadOnlyList<AssistantTopic> topics)
    {
        var ranked = _embeddingMatcher.Rank(freeText, topics.Select(t => new MatchCandidate(t.NodeId, t.Phrases)).ToList());

        if (ranked.Count == 0 || ranked[0].Similarity < _similarityThreshold)
            return TopicResolution.None;

        if (ranked.Count > 1 && ranked[0].Similarity - ranked[1].Similarity < _minimumMargin)
        {
            var closeIds = ranked
                .TakeWhile(r => ranked[0].Similarity - r.Similarity < _minimumMargin)
                .Take(MaxClarifyOptions)
                .Select(r => r.CandidateId)
                .ToHashSet();

            var options = topics
                .Where(t => closeIds.Contains(t.NodeId))
                .Select(t => new AssistantOptionResponse(t.NodeId, t.LabelKey, Array.Empty<string>()))
                .ToList();

            return TopicResolution.Ambiguous(options);
        }

        return TopicResolution.Resolved(ranked[0].CandidateId);
    }
}
