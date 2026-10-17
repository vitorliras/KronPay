using Api.Extensions;
using Application.DTOs.AiChat;
using Application.Executors;
using Application.Pipelines;
using Application.UseCases.AI;
using Application.UseCases.AiChat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Resources;

[Authorize]
[ApiController]
[Route("ai")]
public sealed class AiController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly GetAiStatusUseCase _getStatus;
    private readonly GetCurrentMonthAiUsageUseCase _getCurrentMonthUsage;
    private readonly SendAiChatMessageUseCase _chat;
    private readonly GetAiChatStartersUseCase _starters;
    private readonly IStringLocalizer<Messages> _localizer;

    public AiController(
        UseCaseExecutor executor,
        GetAiStatusUseCase getStatus,
        GetCurrentMonthAiUsageUseCase getCurrentMonthUsage,
        SendAiChatMessageUseCase chat,
        GetAiChatStartersUseCase starters,
        IStringLocalizer<Messages> localizer)
    {
        _executor = executor;
        _getStatus = getStatus;
        _getCurrentMonthUsage = getCurrentMonthUsage;
        _chat = chat;
        _starters = starters;
        _localizer = localizer;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var result = await _executor.ExecuteAsync(_getStatus);

        return result.ToActionResult(_localizer);
    }

    [HttpGet("usage")]
    public async Task<IActionResult> GetCurrentMonthUsage()
    {
        var result = await _executor.ExecuteAsync(_getCurrentMonthUsage);

        return result.ToActionResult(_localizer);
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat(
        AiChatRequest request,
        [FromServices] ValidationPipeline<AiChatRequest, AiChatResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _chat, pipeline);

        return result.ToActionResult(_localizer);
    }

    [HttpGet("chat/starters")]
    public async Task<IActionResult> Starters()
    {
        var result = await _executor.ExecuteAsync(_starters);

        return result.ToActionResult(_localizer);
    }
}
