using Application.Abstractions;
using Application.DTOs.Auth;
using Domain.Enums.Auth;
using Domain.interfaces;
using Domain.Interfaces;
using Domain.Interfaces.Auth;
using Domain.Services.Auth;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Auth;

public sealed class ConfirmEmailUseCase
    : IUseCase<ConfirmEmailRequest, ConfirmEmailResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IVerificationCodeRepository _codeRepository;
    private readonly IVerificationCodeService _codeService;
    private readonly IUnitOfWork _uow;

    public ConfirmEmailUseCase(
        IUserRepository userRepository,
        IVerificationCodeRepository codeRepository,
        IVerificationCodeService codeService,
        IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _codeRepository = codeRepository;
        _codeService = codeService;
        _uow = uow;
    }

    public async Task<ResultEntity<ConfirmEmailResponse>> ExecuteAsync(ConfirmEmailRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null)
            return ResultEntity<ConfirmEmailResponse>.Failure(MessageKeys.InvaldUser);

        if (user.EmailConfirmed)
            return ResultEntity<ConfirmEmailResponse>.Failure(MessageKeys.EmailAlreadyConfirmed);

        var now = DateTime.UtcNow;
        var code = await _codeRepository.GetLastAsync(user.Id, VerificationPurpose.EmailConfirmation);

        if (code is null || code.Used || code.IsExpired(now))
            return ResultEntity<ConfirmEmailResponse>.Failure(MessageKeys.InvalidOrExpiredCode);

        if (code.ExceededMaxAttempts())
            return ResultEntity<ConfirmEmailResponse>.Failure(MessageKeys.TooManyAttempts);

        if (!_codeService.Matches(request.Code, code.CodeHash))
        {
            code.RegisterFailedAttempt();
            _codeRepository.Update(code);
            await _uow.CommitAsync();
            return ResultEntity<ConfirmEmailResponse>.Failure(MessageKeys.InvalidOrExpiredCode);
        }

        code.MarkAsUsed();
        _codeRepository.Update(code);

        user.ConfirmEmail();
        _userRepository.Update(user);

        if (!await _uow.CommitAsync())
            return ResultEntity<ConfirmEmailResponse>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<ConfirmEmailResponse>.Success(
            new ConfirmEmailResponse(true), MessageKeys.OperationSuccess);
    }
}
