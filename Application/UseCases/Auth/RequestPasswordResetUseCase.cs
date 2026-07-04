using Application.Abstractions;
using Application.Abstractions.Email;
using Application.DTOs.Auth;
using Domain.Entities.Auth;
using Domain.Enums.Auth;
using Domain.interfaces;
using Domain.Interfaces;
using Domain.Interfaces.Auth;
using Domain.Services.Auth;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Shared.Localization;
using Shared.Resources;
using Shared.Results;

namespace Application.UseCases.Auth;

public sealed class RequestPasswordResetUseCase
    : IUseCase<RequestPasswordResetRequest, RequestPasswordResetResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IVerificationCodeRepository _codeRepository;
    private readonly IVerificationCodeService _codeService;
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _uow;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly ILogger<RequestPasswordResetUseCase> _logger;

    public RequestPasswordResetUseCase(
        IUserRepository userRepository,
        IVerificationCodeRepository codeRepository,
        IVerificationCodeService codeService,
        IEmailSender emailSender,
        IUnitOfWork uow,
        IStringLocalizer<Messages> localizer,
        ILogger<RequestPasswordResetUseCase> logger)
    {
        _userRepository = userRepository;
        _codeRepository = codeRepository;
        _codeService = codeService;
        _emailSender = emailSender;
        _uow = uow;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<ResultEntity<RequestPasswordResetResponse>> ExecuteAsync(
        RequestPasswordResetRequest request)
    {
        var genericSuccess = ResultEntity<RequestPasswordResetResponse>.Success(
            new RequestPasswordResetResponse(true), MessageKeys.OperationSuccess);

        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user is null)
                return genericSuccess;

            var now = DateTime.UtcNow;
            var lastCode = await _codeRepository.GetLastAsync(user.Id, VerificationPurpose.PasswordReset);

            if (lastCode is not null && !lastCode.Used && _codeService.IsResendCooldownActive(lastCode.CreatedAt, now))
                return genericSuccess;

            if (lastCode is not null && !lastCode.Used)
            {
                lastCode.Invalidate();
                _codeRepository.Update(lastCode);
            }

            var plainCode = _codeService.GenerateCode();
            var codeHash = _codeService.Hash(plainCode);
            var expiresAt = _codeService.ComputeExpiresAt(now);

            var verificationCode = new VerificationCode(
                user.Id, codeHash, VerificationPurpose.PasswordReset, expiresAt);

            var added = await _codeRepository.AddAsync(verificationCode);
            if (!added)
            {
                _logger.LogError(
                    "Falha ao persistir VerificationCode (PasswordReset) para o usuário {UserId}.", user.Id);
                return genericSuccess;
            }

            if (!await _uow.CommitAsync())
            {
                _logger.LogError(
                    "Falha no commit do VerificationCode (PasswordReset) para o usuário {UserId}.", user.Id);
                return genericSuccess;
            }

            await SendEmailBestEffort(user.Id, user.Email.Value, plainCode);

            return genericSuccess;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha inesperada em RequestPasswordResetUseCase para o e-mail informado.");
            return genericSuccess;
        }
    }

    private async Task SendEmailBestEffort(int userId, string email, string plainCode)
    {
        try
        {
            await _emailSender.SendAsync(
                email,
                _localizer[MessageKeys.PasswordResetSubject],
                _localizer[MessageKeys.PasswordResetBody, plainCode]);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha ao enviar e-mail de recuperação de senha para o usuário {UserId}.", userId);
        }
    }
}
