using Api.Extensions;
using Application.DTOs.Planning;
using Application.Executors;
using Application.Pipelines;
using Application.UseCases.Planning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Resources;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("planning")]
public sealed class PlanningController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly GetFinancialProjectionUseCase _getProjection;
    private readonly SimulatePurchaseUseCase _simulatePurchase;
    private readonly GetMonthlyViabilityComparisonUseCase _viabilityComparison;
    private readonly IStringLocalizer<Messages> _localizer;

    public PlanningController(
        UseCaseExecutor executor,
        GetFinancialProjectionUseCase getProjection,
        SimulatePurchaseUseCase simulatePurchase,
        GetMonthlyViabilityComparisonUseCase viabilityComparison,
        IStringLocalizer<Messages> localizer)
    {
        _executor = executor;
        _getProjection = getProjection;
        _simulatePurchase = simulatePurchase;
        _viabilityComparison = viabilityComparison;
        _localizer = localizer;
    }

    [HttpGet("projection")]
    public async Task<IActionResult> GetProjection(
        [FromQuery] GetFinancialProjectionRequest request,
        [FromServices] ValidationPipeline<GetFinancialProjectionRequest, FinancialProjectionResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _getProjection, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpPost("simulate-purchase")]
    public async Task<IActionResult> SimulatePurchase(
        SimulatePurchaseRequest request,
        [FromServices] ValidationPipeline<SimulatePurchaseRequest, PurchaseSimulationResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _simulatePurchase, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpPost("viability-comparison")]
    public async Task<IActionResult> ViabilityComparison(
        MonthlyViabilityComparisonRequest request,
        [FromServices] ValidationPipeline<MonthlyViabilityComparisonRequest, MonthlyViabilityComparisonResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _viabilityComparison, pipeline);
        return result.ToActionResult(_localizer);
    }
}
