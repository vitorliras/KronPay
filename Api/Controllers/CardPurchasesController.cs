using Api.Extensions;
using Application.DTOs.Card.CardPurchases;
using Application.Executors;
using Application.Pipelines;
using Application.UseCases.Card.CardPurchases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Resources;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("card-purchases")]
public sealed class CardPurchasesController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly CreateCardPurchaseUseCase _create;
    private readonly UpdateCardPurchaseUseCase _update;
    private readonly DeactivateCardPurchaseUseCase _deactivate;
    private readonly IStringLocalizer<Messages> _localizer;

    public CardPurchasesController(
        UseCaseExecutor executor,
        CreateCardPurchaseUseCase create,
        UpdateCardPurchaseUseCase update,
        DeactivateCardPurchaseUseCase deactivate,
        IStringLocalizer<Messages> localizer)
    {
        _executor = executor;
        _create = create;
        _update = update;
        _deactivate = deactivate;
        _localizer = localizer;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateCardPurchaseRequest request,
        [FromServices] ValidationPipeline<CreateCardPurchaseRequest, CardPurchaseResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _create, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpPut]
    public async Task<IActionResult> Update(
        UpdateCardPurchaseRequest request,
        [FromServices] ValidationPipeline<UpdateCardPurchaseRequest, CardPurchaseResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _update, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpDelete]
    public async Task<IActionResult> Deactivate(
        DeactivateCardPurchaseRequest request,
        [FromServices] ValidationPipeline<DeactivateCardPurchaseRequest, CardPurchaseResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _deactivate, pipeline);
        return result.ToActionResult(_localizer);
    }
}
