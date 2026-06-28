using Api.Extensions;
using Application.DTOs.Banks;
using Application.Executors;
using Application.Pipelines;
using Application.UseCases.Banks;
using Application.UseCases.CreditCards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Resources;

[Authorize]
[ApiController]
[Route("bank")]
public sealed class BankController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly CreateBankConnectionUseCase _create;
    private readonly GetAllBanksUseCase _getAll;
    private readonly IStringLocalizer<Messages> _localizer;

    public BankController(
        UseCaseExecutor executor,
        CreateBankConnectionUseCase create,
        GetAllBanksUseCase getAll,
        IStringLocalizer<Messages> localizer)
    {
        _executor = executor;
        _create = create;
        _localizer = localizer;
        _getAll = getAll;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateBankConnectionRequest request,
        [FromServices] ValidationPipeline<CreateBankConnectionRequest, BankConnectionResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(
            request,
            _create,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _executor.ExecuteAsync(_getAll);

        return result.ToActionResult(_localizer);
    }

}

