using Api.Extensions;
using Application.DTOs.Card.CardInvoices;
using Application.Executors;
using Application.Pipelines;
using Application.UseCases.Card.CardInvoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Resources;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("card-invoices")]
public sealed class CardInvoicesController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly GetCreditCardSummaryUseCase _getSummary;
    private readonly GetInvoicesByCardUseCase _getByCard;
    private readonly GetCardInvoiceUseCase _getById;
    private readonly GetCardPurchasesByInvoiceUseCase _getItems;
    private readonly PayCardInvoiceUseCase _pay;
    private readonly IStringLocalizer<Messages> _localizer;

    public CardInvoicesController(
        UseCaseExecutor executor,
        GetCreditCardSummaryUseCase getSummary,
        GetInvoicesByCardUseCase getByCard,
        GetCardInvoiceUseCase getById,
        GetCardPurchasesByInvoiceUseCase getItems,
        PayCardInvoiceUseCase pay,
        IStringLocalizer<Messages> localizer)
    {
        _executor = executor;
        _getSummary = getSummary;
        _getByCard = getByCard;
        _getById = getById;
        _getItems = getItems;
        _pay = pay;
        _localizer = localizer;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] GetCreditCardSummaryRequest request,
        [FromServices] ValidationPipeline<GetCreditCardSummaryRequest, CreditCardSummaryResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _getSummary, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpGet]
    public async Task<IActionResult> GetByCard(
        [FromQuery] GetInvoicesByCardRequest request,
        [FromServices] ValidationPipeline<GetInvoicesByCardRequest, IEnumerable<CardInvoiceResponse>> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _getByCard, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        [FromServices] ValidationPipeline<GetCardInvoiceRequest, CardInvoiceResponse> pipeline)
    {
        var request = new GetCardInvoiceRequest(id);
        var result = await _executor.ExecuteAsync(request, _getById, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpGet("{id:int}/items")]
    public async Task<IActionResult> GetItems(
        int id,
        [FromServices] ValidationPipeline<GetCardPurchasesByInvoiceRequest, IEnumerable<CardInstallmentResponse>> pipeline)
    {
        var request = new GetCardPurchasesByInvoiceRequest(id);
        var result = await _executor.ExecuteAsync(request, _getItems, pipeline);
        return result.ToActionResult(_localizer);
    }

    [HttpPost("pay")]
    public async Task<IActionResult> Pay(
        PayCardInvoiceRequest request,
        [FromServices] ValidationPipeline<PayCardInvoiceRequest, CardInvoiceResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _pay, pipeline);
        return result.ToActionResult(_localizer);
    }
}
