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

public sealed class ValidatePasswordResetCodeUseCase
    : IUseCase<ValidateResetCodeRequest, ValidateResetCodeResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IVerificationCodeRepository _codeRepository;
    private readonly IVerificationCodeService _codeService;
    private readonly IUnitOfWork _uow;

    public ValidatePasswordResetCodeUseCase(
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

    public async Task<ResultEntity<ValidateResetCodeResponse>> ExecuteAsync(ValidateResetCodeRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null)
            return ResultEntity<ValidateResetCodeResponse>.Failure(MessageKeys.InvalidOrExpiredCode);

        var now = DateTime.UtcNow;
        var code = await _codeRepository.GetLastAsync(user.Id, VerificationPurpose.PasswordReset);

        if (code is null || code.Used || code.IsExpired(now))
            return ResultEntity<ValidateResetCodeResponse>.Failure(MessageKeys.InvalidOrExpiredCode);

        if (code.ExceededMaxAttempts())
            return ResultEntity<ValidateResetCodeResponse>.Failure(MessageKeys.TooManyAttempts);

        if (!_codeService.Matches(request.Code, code.CodeHash))
        {
            code.RegisterFailedAttempt();
            _codeRepository.Update(code);
            await _uow.CommitAsync();
            return ResultEntity<ValidateResetCodeResponse>.Failure(MessageKeys.InvalidOrExpiredCode);
        }

        return ResultEntity<ValidateResetCodeResponse>.Success(
            new ValidateResetCodeResponse(true), MessageKeys.OperationSuccess);
    }
}
