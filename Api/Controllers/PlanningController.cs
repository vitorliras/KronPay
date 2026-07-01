using Api.Extensions;
using Application.DTOs.Planning;
using Application.Executors;
using Application.Pipelines;
using Application.UseCases.Planning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Resources;
using Shared.Results;

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
    private readonly CreatePlannedCommitmentUseCase _createCommitment;
    private readonly GetAllPlannedCommitmentsUseCase _getCommitments;
    private readonly UpdatePlannedCommitmentUseCase _updateCommitment;
    private readonly DeactivatePlannedCommitmentUseCase _deactivateCommitment;
    private readonly IStringLocalizer<Messages> _localizer;

    public PlanningController(
        UseCaseExecutor executor,
        GetFinancialProjectionUseCase getProjection,
        SimulatePurchaseUseCase simulatePurchase,
        GetMonthlyViabilityComparisonUseCase viabilityComparison,
        CreatePlannedCommitmentUseCase createCommitment,
        GetAllPlannedCommitmentsUseCase getCommitments,
        UpdatePlannedCommitmentUseCase updateCommitment,
        DeactivatePlannedCommitmentUseCase deactivateCommitment,
        IStringLocalizer<Messages> localizer)
    {
        _executor = executor;
        _getProjection = getProjection;
        _simulatePurchase = simulatePurchase;
        _viabilityComparison = viabilityComparison;
        _createCommitment = createCommitment;
        _getCommitments = getCommitments;
        _updateCommitment = updateCommitment;
        _deactivateCommitment = deactivateCommitment;
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

    [HttpGet("commitments")]
    public async Task<IActionResult> GetCommitments()
    {
        var result = await _executor.ExecuteAsync(_getCommitments);
        return result.ToActionResult(_localizer);
    }

    [HttpPost("commitments")]
    public async Task<IActionResult> CreateCommitment(
        CreatePlannedCommitmentRequest request,
        [FromServices] ValidationPipeline<CreatePlannedCommitmentRequest, PlannedCommitmentResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _createCommitment, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpPut("commitments")]
    public async Task<IActionResult> UpdateCommitment(
        UpdatePlannedCommitmentRequest request,
        [FromServices] ValidationPipeline<UpdatePlannedCommitmentRequest, PlannedCommitmentResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _updateCommitment, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpDelete("commitments")]
    public async Task<IActionResult> DeactivateCommitment(
        DeactivatePlannedCommitmentRequest request,
        [FromServices] ValidationPipeline<DeactivatePlannedCommitmentRequest, Unit> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _deactivateCommitment, pipeline);
        return result.ToActionResult(_localizer);
    }
}
