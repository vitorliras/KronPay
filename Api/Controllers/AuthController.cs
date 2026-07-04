using Api.Extensions;
using Application.DTOs.Auth;
using Application.Executors;
using Application.Pipelines;
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
    private readonly UseCaseExecutor _executor;
    private readonly ConfirmEmailUseCase _confirmEmailUseCase;
    private readonly ResendEmailConfirmationCodeUseCase _resendConfirmationCodeUseCase;
    private readonly RequestPasswordResetUseCase _requestPasswordResetUseCase;
    private readonly ValidatePasswordResetCodeUseCase _validatePasswordResetCodeUseCase;
    private readonly ResetPasswordUseCase _resetPasswordUseCase;
    private readonly RefreshTokenUseCase _refreshTokenUseCase;
    private readonly LogoutUseCase _logoutUseCase;
    private readonly IStringLocalizer<Messages> _localizer;


    public AuthController(
        LoginUseCase loginUseCase,
        UseCaseExecutor executor,
        ConfirmEmailUseCase confirmEmailUseCase,
        ResendEmailConfirmationCodeUseCase resendConfirmationCodeUseCase,
        RequestPasswordResetUseCase requestPasswordResetUseCase,
        ValidatePasswordResetCodeUseCase validatePasswordResetCodeUseCase,
        ResetPasswordUseCase resetPasswordUseCase,
        RefreshTokenUseCase refreshTokenUseCase,
        LogoutUseCase logoutUseCase,
        IStringLocalizer<Messages> localizer)
    {
        _loginUseCase = loginUseCase;
        _executor = executor;
        _confirmEmailUseCase = confirmEmailUseCase;
        _resendConfirmationCodeUseCase = resendConfirmationCodeUseCase;
        _requestPasswordResetUseCase = requestPasswordResetUseCase;
        _validatePasswordResetCodeUseCase = validatePasswordResetCodeUseCase;
        _resetPasswordUseCase = resetPasswordUseCase;
        _refreshTokenUseCase = refreshTokenUseCase;
        _logoutUseCase = logoutUseCase;
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

        return result.ToActionResult(_localizer);

    }

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(
        ConfirmEmailRequest request,
        [FromServices] ValidationPipeline<ConfirmEmailRequest, ConfirmEmailResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _confirmEmailUseCase, pipeline);

        return result.ToActionResult(_localizer);
    }

    [HttpPost("resend-confirmation")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendConfirmation(
        ResendCodeRequest request,
        [FromServices] ValidationPipeline<ResendCodeRequest, ResendCodeResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _resendConfirmationCodeUseCase, pipeline);

        return result.ToActionResult(_localizer);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(
        RequestPasswordResetRequest request,
        [FromServices] ValidationPipeline<RequestPasswordResetRequest, RequestPasswordResetResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _requestPasswordResetUseCase, pipeline);

        return result.ToActionResult(_localizer);
    }

    [HttpPost("validate-reset-code")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateResetCode(
        ValidateResetCodeRequest request,
        [FromServices] ValidationPipeline<ValidateResetCodeRequest, ValidateResetCodeResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _validatePasswordResetCodeUseCase, pipeline);

        return result.ToActionResult(_localizer);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request,
        [FromServices] ValidationPipeline<ResetPasswordRequest, ResetPasswordResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _resetPasswordUseCase, pipeline);

        return result.ToActionResult(_localizer);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        RefreshTokenRequest request,
        [FromServices] ValidationPipeline<RefreshTokenRequest, RefreshTokenResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _refreshTokenUseCase, pipeline);

        return result.ToActionResult(_localizer);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(
        LogoutRequest request,
        [FromServices] ValidationPipeline<LogoutRequest, LogoutResponse> pipeline)
    {
        var result = await _executor.ExecuteAsync(request, _logoutUseCase, pipeline);

        return result.ToActionResult(_localizer);
    }
}
