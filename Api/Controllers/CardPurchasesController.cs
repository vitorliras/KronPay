using Api.DTOs.Card;
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
    private readonly ChangeCardPurchaseAmountUseCase _changeAmount;
    private readonly DeactivateCardPurchaseUseCase _deactivate;
    private readonly ParseCardPurchasesFromTextUseCase _parseText;
    private readonly ImportCardPurchasesUseCase _import;
    private readonly CreateCardPurchaseRangeUseCase _createRange;
    private readonly IStringLocalizer<Messages> _localizer;

    public CardPurchasesController(
        UseCaseExecutor executor,
        CreateCardPurchaseUseCase create,
        UpdateCardPurchaseUseCase update,
        ChangeCardPurchaseAmountUseCase changeAmount,
        DeactivateCardPurchaseUseCase deactivate,
        ParseCardPurchasesFromTextUseCase parseText,
        ImportCardPurchasesUseCase import,
        CreateCardPurchaseRangeUseCase createRange,
        IStringLocalizer<Messages> localizer)
    {
        _executor = executor;
        _create = create;
        _update = update;
        _changeAmount = changeAmount;
        _deactivate = deactivate;
        _parseText = parseText;
        _import = import;
        _createRange = createRange;
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

    [HttpPut("amount")]
    public async Task<IActionResult> ChangeAmount(
        ChangeCardPurchaseAmountRequest request,
        [FromServices] ValidationPipeline<ChangeCardPurchaseAmountRequest, CardPurchaseResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _changeAmount, pipeline);
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

    [HttpPost("[action]")]
    public async Task<IActionResult> ParseText(
        ParseCardPurchasesFromTextRequest request,
        [FromServices] ValidationPipeline<ParseCardPurchasesFromTextRequest, CardPurchasePreviewResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _parseText, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpPost("[action]")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Import(
        [FromForm] ImportCardPurchasesFormRequest form,
        [FromServices] ValidationPipeline<ImportCardPurchasesRequest, CardPurchasePreviewResponse> pipeline)
    {
        if (form.File is null || form.File.Length == 0)
            return BadRequest("File is required");

        await using var stream = form.File.OpenReadStream();

        var request = new ImportCardPurchasesRequest(stream, form.File.FileName, form.CreditCardId, form.UseAi);

        var result = await _executor.ExecuteAsync(request, _import, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> CreateRange(
        CreateCardPurchaseRangeRequest request,
        [FromServices] ValidationPipeline<CreateCardPurchaseRangeRequest, CreateCardPurchaseRangeResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _createRange, pipeline);
        return result.ToActionResult(_localizer);
    }
}
