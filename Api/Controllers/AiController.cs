using Api.DTOs.AiChat;
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
    private readonly UploadChatAttachmentsUseCase _uploadAttachments;
    private readonly IStringLocalizer<Messages> _localizer;

    public AiController(
        UseCaseExecutor executor,
        GetAiStatusUseCase getStatus,
        GetCurrentMonthAiUsageUseCase getCurrentMonthUsage,
        SendAiChatMessageUseCase chat,
        GetAiChatStartersUseCase starters,
        UploadChatAttachmentsUseCase uploadAttachments,
        IStringLocalizer<Messages> localizer)
    {
        _executor = executor;
        _getStatus = getStatus;
        _getCurrentMonthUsage = getCurrentMonthUsage;
        _chat = chat;
        _starters = starters;
        _uploadAttachments = uploadAttachments;
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

    [HttpPost("chat/attachments")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(35_000_000)]
    public async Task<IActionResult> UploadAttachments(
        [FromForm] UploadChatAttachmentsFormRequest form,
        [FromServices] ValidationPipeline<UploadChatAttachmentsRequest, UploadChatAttachmentsResponse> pipeline)
    {
        var files = form.Files ?? new List<IFormFile>();
        var streams = files.Select(f => f.OpenReadStream()).ToList();

        try
        {
            var request = new UploadChatAttachmentsRequest(
                files.Select((f, i) => new ChatUploadFile(f.FileName, f.Length, streams[i])).ToList());

            var result = await _executor.ExecuteAsync(request, _uploadAttachments, pipeline);

            return result.ToActionResult(_localizer);
        }
        finally
        {
            foreach (var stream in streams)
                await stream.DisposeAsync();
        }
    }

    [HttpGet("chat/starters")]
    public async Task<IActionResult> Starters()
    {
        var result = await _executor.ExecuteAsync(_starters);

        return result.ToActionResult(_localizer);
    }
}
