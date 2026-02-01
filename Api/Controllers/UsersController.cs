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
    private readonly CreateUserUseCase _useCase;

    public UsersController(
        UseCaseExecutor executor,
        CreateUserUseCase useCase)
    {
        _executor = executor;
        _useCase = useCase;
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
            _useCase,
            pipeline
        );

        return result.ToActionResult(localizer);
    }

}
