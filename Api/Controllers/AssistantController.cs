using Api.Extensions;
using Application.DTOs.Assistant;
using Application.Executors;
using Application.Pipelines;
using Application.UseCases.Assistant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Resources;
using Shared.Results;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("assistant")]
public sealed class AssistantController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly AskAssistantUseCase _ask;
    private readonly IStringLocalizer<Messages> _localizer;

    public AssistantController(
        UseCaseExecutor executor,
        AskAssistantUseCase ask,
        IStringLocalizer<Messages> localizer)
    {
        _executor = executor;
        _ask = ask;
        _localizer = localizer;
    }

    [HttpPost]
    public async Task<IActionResult> Ask(
        AskAssistantRequest request,
        [FromServices] ValidationPipeline<AskAssistantRequest, AssistantNodeResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _ask, pipeline);

        if (result.IsSuccess && result.Value is not null)
            result = ResultEntity<AssistantNodeResponse>.Success(Translate(result.Value), result.Message);

        return result.ToActionResult(_localizer);
    }

    private AssistantNodeResponse Translate(AssistantNodeResponse response)
    {
        var message = _localizer[response.MessageKey, response.MessageArgs.ToArray()];

        var options = response.Options
            .Select(option => option with
            {
                LabelKey = _localizer[option.LabelKey, option.LabelArgs.ToArray()].Value,
            })
            .ToList();

        var navigateTo = response.NavigateTo?
            .Select(nav => nav with { LabelKey = _localizer[nav.LabelKey].Value })
            .ToList();

        return response with { MessageKey = message.Value, Options = options, NavigateTo = navigateTo };
    }
}
