using Api.Extensions;
using Application.Executors;
using Application.UseCases.Gamification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Resources;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("gamification")]
public sealed class GamificationController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly GetUserRankUseCase _getRank;
    private readonly GetUserBadgesUseCase _getBadges;
    private readonly GetMissionsCatalogUseCase _getMissions;
    private readonly GetActiveMissionsProgressUseCase _getMissionsProgress;
    private readonly IStringLocalizer<Messages> _localizer;

    public GamificationController(
        UseCaseExecutor executor,
        GetUserRankUseCase getRank,
        GetUserBadgesUseCase getBadges,
        GetMissionsCatalogUseCase getMissions,
        GetActiveMissionsProgressUseCase getMissionsProgress,
        IStringLocalizer<Messages> localizer)
    {
        _executor = executor;
        _getRank = getRank;
        _getBadges = getBadges;
        _getMissions = getMissions;
        _getMissionsProgress = getMissionsProgress;
        _localizer = localizer;
    }

    [HttpGet("rank")]
    public async Task<IActionResult> GetRank()
    {
        var result = await _executor.ExecuteAsync(_getRank);

        return result.ToActionResult(_localizer);
    }

    [HttpGet("badges")]
    public async Task<IActionResult> GetBadges()
    {
        var result = await _executor.ExecuteAsync(_getBadges);

        return result.ToActionResult(_localizer);
    }

    [HttpGet("missions")]
    public async Task<IActionResult> GetMissions()
    {
        var result = await _executor.ExecuteAsync(_getMissions);

        return result.ToActionResult(_localizer);
    }

    [HttpGet("missions/progress")]
    public async Task<IActionResult> GetMissionsProgress()
    {
        var result = await _executor.ExecuteAsync(_getMissionsProgress);

        return result.ToActionResult(_localizer);
    }
}
