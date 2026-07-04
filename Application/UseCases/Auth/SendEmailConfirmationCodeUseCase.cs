using Application.Abstractions;
using Application.Abstractions.Email;
using Application.DTOs.Auth;
using Domain.Entities.Auth;
using Domain.Enums.Auth;
using Domain.Interfaces;
using Domain.Interfaces.Auth;
using Domain.Services.Auth;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Shared.Localization;
using Shared.Resources;
using Shared.Results;

namespace Application.UseCases.Auth;

public sealed class SendEmailConfirmationCodeUseCase
    : IUseCase<SendEmailConfirmationCodeRequest, SendEmailConfirmationCodeResponse>
{
    private readonly IVerificationCodeRepository _codeRepository;
    private readonly IVerificationCodeService _codeService;
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _uow;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly ILogger<SendEmailConfirmationCodeUseCase> _logger;

    public SendEmailConfirmationCodeUseCase(
        IVerificationCodeRepository codeRepository,
        IVerificationCodeService codeService,
        IEmailSender emailSender,
        IUnitOfWork uow,
        IStringLocalizer<Messages> localizer,
        ILogger<SendEmailConfirmationCodeUseCase> logger)
    {
        _codeRepository = codeRepository;
        _codeService = codeService;
        _emailSender = emailSender;
        _uow = uow;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<ResultEntity<SendEmailConfirmationCodeResponse>> ExecuteAsync(
        SendEmailConfirmationCodeRequest request)
    {
        try
        {
            var now = DateTime.UtcNow;

            var lastCode = await _codeRepository.GetLastAsync(request.UserId, VerificationPurpose.EmailConfirmation);

            if (lastCode is not null && !lastCode.Used && _codeService.IsResendCooldownActive(lastCode.CreatedAt, now))
                return ResultEntity<SendEmailConfirmationCodeResponse>.Failure(MessageKeys.ResendCooldownActive);

            if (lastCode is not null && !lastCode.Used)
            {
                lastCode.Invalidate();
                _codeRepository.Update(lastCode);
            }

            var plainCode = _codeService.GenerateCode();
            var codeHash = _codeService.Hash(plainCode);
            var expiresAt = _codeService.ComputeExpiresAt(now);

            var verificationCode = new VerificationCode(
                request.UserId, codeHash, VerificationPurpose.EmailConfirmation, expiresAt);

            var added = await _codeRepository.AddAsync(verificationCode);
            if (!added)
                return ResultEntity<SendEmailConfirmationCodeResponse>.Failure(MessageKeys.InsertFalied);

            if (!await _uow.CommitAsync())
                return ResultEntity<SendEmailConfirmationCodeResponse>.Failure(MessageKeys.DataPersistenceFailed);

            await SendEmailBestEffort(request.UserId, request.Email, plainCode);

            return ResultEntity<SendEmailConfirmationCodeResponse>.Success(
                new SendEmailConfirmationCodeResponse(true), MessageKeys.OperationSuccess);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha inesperada em SendEmailConfirmationCodeUseCase para o usuário {UserId}.", request.UserId);
            return ResultEntity<SendEmailConfirmationCodeResponse>.Failure(MessageKeys.OperationFailed);
        }
    }

    private async Task SendEmailBestEffort(int userId, string email, string plainCode)
    {
        try
        {
            await _emailSender.SendAsync(
                email,
                _localizer[MessageKeys.EmailConfirmationSubject],
                _localizer[MessageKeys.EmailConfirmationBody, plainCode]);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha ao enviar e-mail de confirmação para o usuário {UserId}.", userId);
        }
    }
}
