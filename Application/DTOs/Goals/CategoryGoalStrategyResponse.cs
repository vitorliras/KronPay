namespace Application.DTOs.Goals;

public sealed record CategoryGoalStrategyResponse(
    bool GroupedBySubcategory,
    IReadOnlyList<CategorySpendingGroupResponse> Groups,
    CategorySpendingTrendResponse Trend,
    string SuggestionMessageKey,
    IReadOnlyDictionary<string, string> SuggestionArgs);
