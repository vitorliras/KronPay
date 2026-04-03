using Api.Extensions;
using Application.DTOs.Users;
using Application.Executors;
using Application.Pipelines;
using Application.UseCases.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Resources;

[Authorize]
[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly UseCaseExecutor _executor;
    private readonly CreateUserUseCase _createUseCase;
    private readonly GetUserUseCase _getUseCase;
    private readonly IStringLocalizer<Messages> _localizer;


    public UsersController(
        UseCaseExecutor executor,
        CreateUserUseCase createUseCase,
        GetUserUseCase getUseCase,
        IStringLocalizer<Messages> localizer)
    {
        _executor = executor;
        _createUseCase = createUseCase;
        _getUseCase = getUseCase;
        _localizer = localizer;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create(
    CreateUserRequest request,
    [FromServices] ValidationPipeline<CreateUserRequest, UserResponse> pipeline,
    [FromServices] IStringLocalizer<Messages> localizer)
    {

        var result = await _executor.ExecuteAsync(
            request,
            _createUseCase,
            pipeline
        );

        return result.ToActionResult(localizer);
    }

    [HttpGet]
    public async Task<IActionResult> GetUser()  
    {
        var result = await _executor.ExecuteAsync(_getUseCase);

        return result.ToActionResult(_localizer);
    }

}
