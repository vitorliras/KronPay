using Application.DTOs.Assistant;
using Domain.Interfaces;
using Domain.Interfaces.Transactions;
using Domain.Services.Assistant;
using Shared.Localization;

namespace Application.Services.Assistant;

public sealed class CategoryParameterResolver
{
    private const int MaxVisibleOptions = 5;

    private readonly ICategoryRepository _categoryRepository;
    private readonly ICategoryItemRepository _categoryItemRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IParameterFuzzyMatcher _fuzzyMatcher;

    public CategoryParameterResolver(
        ICategoryRepository categoryRepository,
        ICategoryItemRepository categoryItemRepository,
        ITransactionRepository transactionRepository,
        IParameterFuzzyMatcher fuzzyMatcher)
    {
        _categoryRepository = categoryRepository;
        _categoryItemRepository = categoryItemRepository;
        _transactionRepository = transactionRepository;
        _fuzzyMatcher = fuzzyMatcher;
    }

    public async Task<IReadOnlyList<AssistantOptionResponse>> BuildTopOptionsAsync(int userId)
    {
        var candidates = await BuildCandidatesAsync(userId);
        var now = DateTime.UtcNow;
        var transactions = await _transactionRepository.GetByMonthAsync(userId, now.Year, now.Month);

        var usageByCandidateId = transactions
            .Where(t => t.CodTypeTransaction == "E" && t.Status != "C")
            .GroupBy(t => CandidateId(t.CategoryId, t.CategoryItemId))
            .ToDictionary(g => g.Key, g => g.Count());

        var ranked = candidates
            .OrderByDescending(c => usageByCandidateId.TryGetValue(c.Id, out var count) ? count : 0)
            .ThenBy(c => c.DisplayName)
            .ToList();

        if (ranked.Count <= MaxVisibleOptions)
            return ToOptions(ranked);

        var visible = ToOptions(ranked.Take(MaxVisibleOptions)).ToList();
        visible.Add(new AssistantOptionResponse("categories:more", MessageKeys.AssistantOptionMore, Array.Empty<string>()));
        return visible;
    }

    public async Task<CategoryParameterMatch?> ResolveAsync(string freeText, int userId)
    {
        var candidates = await BuildCandidatesAsync(userId);
        if (candidates.Count == 0)
            return null;

        var matchCandidates = candidates
            .Select(c => new MatchCandidate(c.Id, new[] { c.DisplayName }))
            .ToList();

        var resolvedId = _fuzzyMatcher.Match(freeText, matchCandidates)?.CandidateId;

        if (resolvedId is null)
            return null;

        return ToMatch(candidates.FirstOrDefault(c => c.Id == resolvedId));
    }

    public async Task<CategoryParameterMatch?> ResolveByOptionIdAsync(string optionId, int userId)
    {
        var candidates = await BuildCandidatesAsync(userId);
        return ToMatch(candidates.FirstOrDefault(c => c.Id == optionId));
    }

    private static CategoryParameterMatch? ToMatch(CategoryCandidate? candidate) =>
        candidate is null ? null : new CategoryParameterMatch(candidate.CategoryId, candidate.CategoryItemId, candidate.DisplayName);

    private static IReadOnlyList<AssistantOptionResponse> ToOptions(IEnumerable<CategoryCandidate> candidates) =>
        candidates
            .Select(c => new AssistantOptionResponse(c.Id, MessageKeys.AssistantDynamicLabel, new[] { c.DisplayName }))
            .ToList();

    private async Task<IReadOnlyList<CategoryCandidate>> BuildCandidatesAsync(int userId)
    {
        var categories = await _categoryRepository.GetAllAsync(userId);
        var categoryItems = await _categoryItemRepository.GetAllByUserIdAsync(userId);

        var candidates = new List<CategoryCandidate>();
        candidates.AddRange(categories
            .Where(c => c.Active)
            .Select(c => new CategoryCandidate(CandidateId(c.Id, null), c.Id, null, c.Description)));

        candidates.AddRange(categoryItems
            .Where(i => i.Active)
            .Select(i => new CategoryCandidate(CandidateId(null, i.Id), i.CategoryId, i.Id, i.Description)));

        return candidates;
    }

    private static string CandidateId(int? categoryId, int? categoryItemId) =>
        categoryItemId is not null ? $"categoryitem:{categoryItemId}" : $"category:{categoryId}";

    private sealed record CategoryCandidate(string Id, int CategoryId, int? CategoryItemId, string DisplayName);
}
