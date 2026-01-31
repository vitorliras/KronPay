using Application.DTOs.Auth;
using Application.UseCases.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Localization;
using Shared.Resources;
using Shared.Results;

namespace Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly LoginUseCase _loginUseCase;
    private readonly IStringLocalizer<Messages> _localizer;


    public AuthController(LoginUseCase loginUseCase, IStringLocalizer<Messages> localizer)
    {
        _loginUseCase = loginUseCase;
        _localizer = localizer;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _loginUseCase.ExecuteAsync(request);

        if (result.IsFailure)
        {
            var message = _localizer[result.ErrorCode].Value;

            return Unauthorized(new
            {
                message
            });
        }

        return Ok(result.Value);
    }
}
