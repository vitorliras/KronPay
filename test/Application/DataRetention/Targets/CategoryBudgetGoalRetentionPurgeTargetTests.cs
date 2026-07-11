using Application.DataRetention.Targets;
using Domain.Entities.Goals;
using Domain.Enums.Goals;
using Domain.Interfaces.Goals;
using Moq;
using Shouldly;

namespace Tests.Application.DataRetention.Targets;

public class CategoryBudgetGoalRetentionPurgeTargetTests
{
    [Fact]
    public async Task Apaga_metas_desativadas_antes_do_corte()
    {
        var cutoff = new DateTime(2026, 7, 1);
        var goal = CreateDeactivatedGoal();

        var repository = new Mock<ICategoryBudgetGoalRepository>();
        repository.Setup(r => r.GetDeactivatedOlderThanAsync(cutoff))
            .ReturnsAsync(new List<CategoryBudgetGoal> { goal });

        var sut = new CategoryBudgetGoalRetentionPurgeTarget(repository.Object);

        var removed = await sut.PurgeAsync(cutoff);

        removed.ShouldBe(1);
        repository.Verify(r => r.DeleteRangeAsync(It.Is<IEnumerable<CategoryBudgetGoal>>(g => g.Contains(goal))), Times.Once);
    }

    [Fact]
    public async Task Nao_apaga_nada_quando_nao_ha_metas_desativadas()
    {
        var cutoff = new DateTime(2026, 7, 1);

        var repository = new Mock<ICategoryBudgetGoalRepository>();
        repository.Setup(r => r.GetDeactivatedOlderThanAsync(cutoff))
            .ReturnsAsync(new List<CategoryBudgetGoal>());

        var sut = new CategoryBudgetGoalRetentionPurgeTarget(repository.Object);

        var removed = await sut.PurgeAsync(cutoff);

        removed.ShouldBe(0);
        repository.Verify(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<CategoryBudgetGoal>>()), Times.Never);
    }

    private static CategoryBudgetGoal CreateDeactivatedGoal()
    {
        var goal = new CategoryBudgetGoal(1, 1, 500m, GoalPriority.Medium);
        goal.Deactivate();
        return goal;
    }
}
