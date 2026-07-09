using Domain.Services.Assistant;
using Microsoft.Extensions.Configuration;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;

namespace Infrastructure.Services.Assistant;

public sealed class OnnxIntentEmbeddingMatcher : IIntentEmbeddingMatcher, IDisposable
{
    private readonly InferenceSession _session;
    private readonly SentencePieceTokenizer _tokenizer;
    private readonly double _similarityThreshold;

    public OnnxIntentEmbeddingMatcher(IConfiguration configuration)
    {
        var modelPath = ResolvePath(configuration["Assistant:EmbeddingModelPath"]
            ?? throw new InvalidOperationException("Assistant:EmbeddingModelPath não configurado."));
        var tokenizerPath = ResolvePath(configuration["Assistant:TokenizerModelPath"]
            ?? throw new InvalidOperationException("Assistant:TokenizerModelPath não configurado."));

        _similarityThreshold = configuration.GetValue<double?>("Assistant:SimilarityThreshold") ?? 0.75;
        _session = new InferenceSession(modelPath);

        using var tokenizerStream = File.OpenRead(tokenizerPath);
        _tokenizer = SentencePieceTokenizer.Create(tokenizerStream, addBeginningOfSentence: true, addEndOfSentence: true);
    }

    private static string ResolvePath(string configuredPath) =>
        Path.IsPathRooted(configuredPath) ? configuredPath : Path.Combine(AppContext.BaseDirectory, configuredPath);

    public MatchResult? Match(string inputText, IReadOnlyCollection<MatchCandidate> candidates)
    {
        var best = Rank(inputText, candidates).FirstOrDefault();
        return best is not null && best.Similarity >= _similarityThreshold ? best : null;
    }

    public IReadOnlyList<MatchResult> Rank(string inputText, IReadOnlyCollection<MatchCandidate> candidates)
    {
        if (string.IsNullOrWhiteSpace(inputText) || candidates.Count == 0)
            return Array.Empty<MatchResult>();

        var inputEmbedding = ComputeEmbedding(inputText);

        return candidates
            .Select(candidate =>
            {
                var similarity = candidate.Terms
                    .Select(term => CosineSimilarity(inputEmbedding, ComputeEmbedding(term)))
                    .DefaultIfEmpty(double.MinValue)
                    .Max();

                return new MatchResult(candidate.Id, similarity);
            })
            .OrderByDescending(result => result.Similarity)
            .ToList();
    }

    private float[] ComputeEmbedding(string text)
    {
        var ids = _tokenizer.EncodeToIds(text.ToLowerInvariant(), addBeginningOfSentence: true, addEndOfSentence: true);
        var length = ids.Count;

        var inputIds = new DenseTensor<long>(new[] { 1, length });
        var attentionMask = new DenseTensor<long>(new[] { 1, length });
        var tokenTypeIds = new DenseTensor<long>(new[] { 1, length });

        for (var i = 0; i < length; i++)
        {
            inputIds[0, i] = ids[i];
            attentionMask[0, i] = 1;
            tokenTypeIds[0, i] = 0;
        }

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask),
            NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds),
        };

        using var results = _session.Run(inputs);
        var lastHiddenState = results.First(result => result.Name == "last_hidden_state").AsTensor<float>();

        var hiddenSize = lastHiddenState.Dimensions[2];
        var pooled = new float[hiddenSize];

        for (var i = 0; i < length; i++)
        {
            for (var j = 0; j < hiddenSize; j++)
                pooled[j] += lastHiddenState[0, i, j];
        }

        for (var j = 0; j < hiddenSize; j++)
            pooled[j] /= length;

        Normalize(pooled);
        return pooled;
    }

    private static void Normalize(float[] vector)
    {
        var normSquared = 0.0;
        foreach (var value in vector)
            normSquared += value * value;

        var norm = Math.Sqrt(normSquared);
        if (norm == 0)
            return;

        for (var i = 0; i < vector.Length; i++)
            vector[i] = (float)(vector[i] / norm);
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0;
        for (var i = 0; i < a.Length; i++)
            dot += a[i] * b[i];

        return dot;
    }

    public void Dispose() => _session.Dispose();
}
