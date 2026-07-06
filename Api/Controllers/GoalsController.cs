using Api.Extensions;
using Application.DTOs.Goals;
using Application.Executors;
using Application.Pipelines;
using Application.UseCases.Goals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Resources;
using Shared.Results;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("goals")]
public sealed class GoalsController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly GetGoalsOverviewUseCase _getOverview;
    private readonly CreateFinancialGoalUseCase _createFinancialGoal;
    private readonly UpdateFinancialGoalUseCase _updateFinancialGoal;
    private readonly CancelFinancialGoalUseCase _cancelFinancialGoal;
    private readonly RegisterFinancialGoalContributionUseCase _registerContribution;
    private readonly CreateCategoryBudgetGoalUseCase _createCategoryBudgetGoal;
    private readonly UpdateCategoryBudgetGoalUseCase _updateCategoryBudgetGoal;
    private readonly DeactivateCategoryBudgetGoalUseCase _deactivateCategoryBudgetGoal;
    private readonly GetFinancialGoalDetailUseCase _getFinancialGoalDetail;
    private readonly SimulateExtraContributionUseCase _simulateExtraContribution;
    private readonly GetFinancialGoalsHistoryUseCase _getHistory;
    private readonly RetryFinancialGoalUseCase _retry;
    private readonly IStringLocalizer<Messages> _localizer;

    public GoalsController(
        UseCaseExecutor executor,
        GetGoalsOverviewUseCase getOverview,
        CreateFinancialGoalUseCase createFinancialGoal,
        UpdateFinancialGoalUseCase updateFinancialGoal,
        CancelFinancialGoalUseCase cancelFinancialGoal,
        RegisterFinancialGoalContributionUseCase registerContribution,
        CreateCategoryBudgetGoalUseCase createCategoryBudgetGoal,
        UpdateCategoryBudgetGoalUseCase updateCategoryBudgetGoal,
        DeactivateCategoryBudgetGoalUseCase deactivateCategoryBudgetGoal,
        GetFinancialGoalDetailUseCase getFinancialGoalDetail,
        SimulateExtraContributionUseCase simulateExtraContribution,
        GetFinancialGoalsHistoryUseCase getHistory,
        RetryFinancialGoalUseCase retry,
        IStringLocalizer<Messages> localizer)
    {
        _executor = executor;
        _getOverview = getOverview;
        _createFinancialGoal = createFinancialGoal;
        _updateFinancialGoal = updateFinancialGoal;
        _cancelFinancialGoal = cancelFinancialGoal;
        _registerContribution = registerContribution;
        _createCategoryBudgetGoal = createCategoryBudgetGoal;
        _updateCategoryBudgetGoal = updateCategoryBudgetGoal;
        _deactivateCategoryBudgetGoal = deactivateCategoryBudgetGoal;
        _getFinancialGoalDetail = getFinancialGoalDetail;
        _simulateExtraContribution = simulateExtraContribution;
        _getHistory = getHistory;
        _retry = retry;
        _localizer = localizer;
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Overview()
    {
        var result = await _executor.ExecuteAsync(_getOverview);
        return result.ToActionResult(_localizer);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> CreateFinancialGoal(
        CreateFinancialGoalRequest request,
        [FromServices] ValidationPipeline<CreateFinancialGoalRequest, FinancialGoalResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _createFinancialGoal, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpPut("[action]")]
    public async Task<IActionResult> UpdateFinancialGoal(
        UpdateFinancialGoalRequest request,
        [FromServices] ValidationPipeline<UpdateFinancialGoalRequest, FinancialGoalResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _updateFinancialGoal, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpDelete("[action]")]
    public async Task<IActionResult> CancelFinancialGoal(
        CancelFinancialGoalRequest request,
        [FromServices] ValidationPipeline<CancelFinancialGoalRequest, Unit> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _cancelFinancialGoal, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> RegisterContribution(
        RegisterContributionRequest request,
        [FromServices] ValidationPipeline<RegisterContributionRequest, ContributionResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _registerContribution, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> CreateCategoryBudgetGoal(
        CreateCategoryBudgetGoalRequest request,
        [FromServices] ValidationPipeline<CreateCategoryBudgetGoalRequest, CategoryBudgetGoalResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _createCategoryBudgetGoal, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpPut("[action]")]
    public async Task<IActionResult> UpdateCategoryBudgetGoal(
        UpdateCategoryBudgetGoalRequest request,
        [FromServices] ValidationPipeline<UpdateCategoryBudgetGoalRequest, CategoryBudgetGoalResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _updateCategoryBudgetGoal, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpDelete("[action]")]
    public async Task<IActionResult> DeactivateCategoryBudgetGoal(
        DeactivateCategoryBudgetGoalRequest request,
        [FromServices] ValidationPipeline<DeactivateCategoryBudgetGoalRequest, Unit> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _deactivateCategoryBudgetGoal, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpGet("financial/{goalId:int}/[action]")]
    public async Task<IActionResult> Detail(
        int goalId,
        [FromServices] ValidationPipeline<GetFinancialGoalDetailRequest, GoalViabilityResponse> pipeline)
    {
        var request = new GetFinancialGoalDetailRequest(goalId);

        var result = await _executor.ExecuteAsync(request, _getFinancialGoalDetail, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> SimulateExtraContribution(
        SimulateExtraContributionRequest request,
        [FromServices] ValidationPipeline<SimulateExtraContributionRequest, SimulateExtraContributionResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _simulateExtraContribution, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpGet("financial/[action]")]
    public async Task<IActionResult> History(
        [FromQuery] string? search,
        [FromServices] ValidationPipeline<GetGoalsHistoryRequest, GoalHistoryResponse> pipeline)
    {
        var request = new GetGoalsHistoryRequest(search);

        var result = await _executor.ExecuteAsync(request, _getHistory, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> Retry(
        RetryFinancialGoalRequest request,
        [FromServices] ValidationPipeline<RetryFinancialGoalRequest, FinancialGoalResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _retry, pipeline);
        return result.ToActionResult(_localizer);
    }
}
