using Api.Extensions;
using Application.DTOs.Configuration.CreditCards;
using Application.Executors;
using Application.Pipelines;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Resources;
using Shared.Results;
using Application.UseCases.CreditCards;
using Application.UseCases.creditCards;

[Authorize]
[ApiController]
[Route("creditcards")]
public sealed class CreditCardsController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly CreateCreditCardUseCase _create;
    private readonly UpdateCreditCardUseCase _update;
    private readonly GetAllCreditCardUseCase _getAll;
    private readonly GetCreditCardByIdUseCase _getById;
    private readonly DeactivateCreditCardUseCase _deactivate;
    private readonly IStringLocalizer<Messages> _localizer;

    public CreditCardsController(
        UseCaseExecutor executor,
        CreateCreditCardUseCase create,
        UpdateCreditCardUseCase update,
        DeactivateCreditCardUseCase deactivate,
        GetAllCreditCardUseCase getAll,
        GetCreditCardByIdUseCase getById,
        IStringLocalizer<Messages> localizer)
    {
        _executor = executor;
        _create = create;
        _update = update;
        _deactivate = deactivate;
        _getAll = getAll;
        _getById = getById;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _executor.ExecuteAsync(_getAll);

        return result.ToActionResult(_localizer);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        [FromQuery] int userId,
        [FromServices] ValidationPipeline<CreditCardIdRequest, CreditCardResponse> pipeline)
    {
        var request = new CreditCardIdRequest(id);

        var result = await _executor.ExecuteAsync(
            request,
            _getById,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateCreditCardRequest request,
        [FromServices] ValidationPipeline<CreateCreditCardRequest, CreditCardResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(
            request,
            _create,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

    [HttpPut]
    public async Task<IActionResult> Update(
        UpdateCreditCardRequest request,
        [FromServices] ValidationPipeline<UpdateCreditCardRequest, CreditCardResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(
            request,
            _update,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

    [HttpDelete]
    public async Task<IActionResult> Deactivate(
    CreditCardIdRequest request,
    [FromServices] ValidationPipeline<CreditCardIdRequest, Unit> pipeline)
    {

        var result = await _executor.ExecuteAsync(
            request,
            _deactivate,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

}
