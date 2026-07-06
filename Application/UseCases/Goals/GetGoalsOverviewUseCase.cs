using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Goals;
using Domain.Interfaces;
using Domain.Interfaces.Goals;
using Domain.Interfaces.Transactions;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Goals;

public sealed class GetGoalsOverviewUseCase : IUseCaseWithoutRequest<GoalsOverviewResponse>
{
    private readonly IFinancialGoalRepository _financialGoalRepository;
    private readonly ICategoryBudgetGoalRepository _categoryBudgetGoalRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetGoalsOverviewUseCase(
        IFinancialGoalRepository financialGoalRepository,
        ICategoryBudgetGoalRepository categoryBudgetGoalRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _financialGoalRepository = financialGoalRepository;
        _categoryBudgetGoalRepository = categoryBudgetGoalRepository;
        _transactionRepository = transactionRepository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<GoalsOverviewResponse>> ExecuteAsync()
    {
        var userId = _currentUser.UserId;
        var now = DateTime.UtcNow;

        var financialGoals = (await _financialGoalRepository.GetActiveAsync(userId)).ToList();

        var expiredAny = false;
        foreach (var goal in financialGoals.Where(g => g.IsPastDue(now)))
        {
            goal.MarkAsExpired();
            _financialGoalRepository.Update(goal);
            expiredAny = true;
        }

        if (expiredAny)
            await _uow.CommitAsync();

        var financialGoalsResponse = financialGoals
            .Where(g => g.Status == Domain.Enums.Goals.FinancialGoalStatus.Active)
            .Select(g => new FinancialGoalResponse(
                g.Id,
                g.Description,
                g.TargetAmount,
                g.CurrentAmount,
                g.TargetDate,
                g.Priority,
                g.Status,
                g.CreatedAt,
                g.CompletedAt,
                g.PreviousAttemptGoalId));

        var categoryBudgetGoals = await _categoryBudgetGoalRepository.GetActiveAsync(userId);
        var monthTransactions = await _transactionRepository.GetByMonthAsync(userId, now.Year, now.Month);

        var categoryBudgetGoalsResponse = new List<CategoryBudgetGoalResponse>();
        foreach (var goal in categoryBudgetGoals)
        {
            var spent = monthTransactions
                .Where(t => t.CategoryId == goal.CategoryId && t.CodTypeTransaction == "E" && t.Status != "C")
                .Sum(t => t.Amount);

            categoryBudgetGoalsResponse.Add(new CategoryBudgetGoalResponse(
                goal.Id,
                goal.CategoryId,
                goal.MonthlyLimit,
                goal.Priority,
                goal.Active,
                spent));
        }

        return ResultEntity<GoalsOverviewResponse>.Success(
            new GoalsOverviewResponse(financialGoalsResponse, categoryBudgetGoalsResponse),
            MessageKeys.OperationSuccess);
    }
}
