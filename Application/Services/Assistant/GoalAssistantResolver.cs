using Application.DTOs.Assistant;
using Domain.Entities.Goals;
using Domain.Interfaces.Goals;
using Shared.Localization;

namespace Application.Services.Assistant;

public sealed class GoalAssistantResolver
{
    private const int DescriptionMaxLength = 40;

    private readonly IFinancialGoalRepository _goalRepository;

    public GoalAssistantResolver(IFinancialGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task<IReadOnlyList<FinancialGoal>> GetActiveGoalsAsync(int userId) =>
        (await _goalRepository.GetActiveAsync(userId)).ToList();

    public async Task<FinancialGoal?> GetByIdAsync(int goalId, int userId) =>
        await _goalRepository.GetByIdAsync(goalId, userId);

    public IReadOnlyList<AssistantOptionResponse> BuildSelectOptions(IReadOnlyList<FinancialGoal> goals) =>
        goals
            .Select(g => new AssistantOptionResponse(
                $"goal:{g.Id}",
                MessageKeys.AssistantDynamicLabel,
                new[] { AssistantTextHelper.Truncate(g.Description, DescriptionMaxLength) }))
            .ToList();
}
