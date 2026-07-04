using Application.Abstractions;
using Application.DTOs.Auth;
using Domain.interfaces;
using Microsoft.Extensions.Logging;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Auth;

public sealed class ResendEmailConfirmationCodeUseCase
    : IUseCase<ResendCodeRequest, ResendCodeResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly SendEmailConfirmationCodeUseCase _sendCodeUseCase;
    private readonly ILogger<ResendEmailConfirmationCodeUseCase> _logger;

    public ResendEmailConfirmationCodeUseCase(
        IUserRepository userRepository,
        SendEmailConfirmationCodeUseCase sendCodeUseCase,
        ILogger<ResendEmailConfirmationCodeUseCase> logger)
    {
        _userRepository = userRepository;
        _sendCodeUseCase = sendCodeUseCase;
        _logger = logger;
    }

    public async Task<ResultEntity<ResendCodeResponse>> ExecuteAsync(ResendCodeRequest request)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user is null)
                return ResultEntity<ResendCodeResponse>.Failure(MessageKeys.InvaldUser);

            if (user.EmailConfirmed)
                return ResultEntity<ResendCodeResponse>.Failure(MessageKeys.EmailAlreadyConfirmed);

            var result = await _sendCodeUseCase.ExecuteAsync(
                new SendEmailConfirmationCodeRequest(user.Id, user.Email.Value));

            if (!result.IsSuccess)
                return ResultEntity<ResendCodeResponse>.Failure(result.Message!);

            return ResultEntity<ResendCodeResponse>.Success(
                new ResendCodeResponse(true), MessageKeys.OperationSuccess);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha inesperada em ResendEmailConfirmationCodeUseCase.");
            return ResultEntity<ResendCodeResponse>.Failure(MessageKeys.OperationFailed);
        }
    }
}
