using Api.Extensions;
using Application.DTOs.Configuration.PaymentMethods;
using Application.Executors;
using Application.Pipelines;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Resources;
using Shared.Results;
using Application.UseCases.PaymentMethods;

[Authorize]
[ApiController]
[Route("paymentmethods")]
public sealed class PaymentMethodsController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly CreatePaymentMethodUseCase _create;
    private readonly UpdatePaymentMethodUseCase _update;
    private readonly GetAllPaymentMethodUseCase _getAll;
    private readonly GetPaymentMethodByIdUseCase _getById;
    private readonly DeactivatePaymentMethodUseCase _deactivate;
    private readonly IStringLocalizer<Messages> _localizer;

    public PaymentMethodsController(
        UseCaseExecutor executor,
        CreatePaymentMethodUseCase create,
        UpdatePaymentMethodUseCase update,
        DeactivatePaymentMethodUseCase deactivate,
        GetAllPaymentMethodUseCase getAll,
        GetPaymentMethodByIdUseCase getById,
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
        [FromServices] ValidationPipeline<PaymentMethodIdRequest, PaymentMethodResponse> pipeline)
    {
        var request = new PaymentMethodIdRequest(id);

        var result = await _executor.ExecuteAsync(
            request,
            _getById,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreatePaymentMethodRequest request,
        [FromServices] ValidationPipeline<CreatePaymentMethodRequest, PaymentMethodResponse> pipeline)
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
        UpdatePaymentMethodRequest request,
        [FromServices] ValidationPipeline<UpdatePaymentMethodRequest, PaymentMethodResponse> pipeline)
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
    PaymentMethodIdRequest request,
    [FromServices] ValidationPipeline<PaymentMethodIdRequest, Unit> pipeline)
    {

        var result = await _executor.ExecuteAsync(
            request,
            _deactivate,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

}
