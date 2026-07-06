using Api.Extensions;
using Application.DTOs.Notifications;
using Application.Executors;
using Application.Pipelines;
using Application.UseCases.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Resources;

[Authorize]
[ApiController]
[Route("notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly GetNotificationsUseCase _getAll;
    private readonly GetArchivedNotificationsUseCase _getArchived;
    private readonly GetCriticalUnresolvedNotificationsUseCase _getCritical;
    private readonly GetUnreadNotificationCountUseCase _getUnreadCount;
    private readonly MarkNotificationAsReadUseCase _markAsRead;
    private readonly GetNotificationPreferencesUseCase _getPreferences;
    private readonly UpdateNotificationPreferencesUseCase _updatePreferences;
    private readonly IStringLocalizer<Messages> _localizer;

    public NotificationsController(
        UseCaseExecutor executor,
        GetNotificationsUseCase getAll,
        GetArchivedNotificationsUseCase getArchived,
        GetCriticalUnresolvedNotificationsUseCase getCritical,
        GetUnreadNotificationCountUseCase getUnreadCount,
        MarkNotificationAsReadUseCase markAsRead,
        GetNotificationPreferencesUseCase getPreferences,
        UpdateNotificationPreferencesUseCase updatePreferences,
        IStringLocalizer<Messages> localizer)
    {
        _executor = executor;
        _getAll = getAll;
        _getArchived = getArchived;
        _getCritical = getCritical;
        _getUnreadCount = getUnreadCount;
        _markAsRead = markAsRead;
        _getPreferences = getPreferences;
        _updatePreferences = updatePreferences;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _executor.ExecuteAsync(_getAll);

        return result.ToActionResult(_localizer);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Archived()
    {
        var result = await _executor.ExecuteAsync(_getArchived);

        return result.ToActionResult(_localizer);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Critical()
    {
        var result = await _executor.ExecuteAsync(_getCritical);

        return result.ToActionResult(_localizer);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount()
    {
        var result = await _executor.ExecuteAsync(_getUnreadCount);

        return result.ToActionResult(_localizer);
    }

    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(
        int id,
        [FromServices] ValidationPipeline<MarkNotificationAsReadRequest, NotificationResponse> pipeline)
    {
        var request = new MarkNotificationAsReadRequest(id);

        var result = await _executor.ExecuteAsync(
            request,
            _markAsRead,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences()
    {
        var result = await _executor.ExecuteAsync(_getPreferences);

        return result.ToActionResult(_localizer);
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences(
        UpdateNotificationPreferencesRequest request,
        [FromServices] ValidationPipeline<UpdateNotificationPreferencesRequest, NotificationPreferenceResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(
            request,
            _updatePreferences,
            pipeline
        );

        return result.ToActionResult(_localizer);
    }
}
