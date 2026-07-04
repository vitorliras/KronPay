using Application.Abstractions;
using Application.Abstractions.Auth;
using Application.DTOs.Auth;
using Domain.Enums.Auth;
using Domain.interfaces;
using Domain.Interfaces;
using Domain.Interfaces.Auth;
using Domain.ValueObjects.User;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Auth;

public sealed class ResetPasswordUseCase
    : IUseCase<ResetPasswordRequest, ResetPasswordResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IVerificationCodeRepository _codeRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ValidatePasswordResetCodeUseCase _validateCodeUseCase;
    private readonly IUnitOfWork _uow;

    public ResetPasswordUseCase(
        IUserRepository userRepository,
        IVerificationCodeRepository codeRepository,
        IPasswordHasher passwordHasher,
        ValidatePasswordResetCodeUseCase validateCodeUseCase,
        IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _codeRepository = codeRepository;
        _passwordHasher = passwordHasher;
        _validateCodeUseCase = validateCodeUseCase;
        _uow = uow;
    }

    public async Task<ResultEntity<ResetPasswordResponse>> ExecuteAsync(ResetPasswordRequest request)
    {
        try
        {
            var validation = await _validateCodeUseCase.ExecuteAsync(
                new ValidateResetCodeRequest(request.Email, request.Code));

            if (!validation.IsSuccess)
                return ResultEntity<ResetPasswordResponse>.Failure(validation.Message!);

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user is null)
                return ResultEntity<ResetPasswordResponse>.Failure(MessageKeys.InvaldUser);

            var code = await _codeRepository.GetLastAsync(user.Id, VerificationPurpose.PasswordReset);
            if (code is null)
                return ResultEntity<ResetPasswordResponse>.Failure(MessageKeys.InvalidOrExpiredCode);

            var password = new Password(request.NewPassword);
            var passwordHash = _passwordHasher.Hash(password.Value);

            user.ChangePassword(passwordHash);
            _userRepository.Update(user);

            code.MarkAsUsed();
            _codeRepository.Update(code);

            if (!await _uow.CommitAsync())
                return ResultEntity<ResetPasswordResponse>.Failure(MessageKeys.DataPersistenceFailed);

            return ResultEntity<ResetPasswordResponse>.Success(
                new ResetPasswordResponse(true), MessageKeys.OperationSuccess);
        }
        catch (Domain.Exceptions.DomainException e)
        {
            return ResultEntity<ResetPasswordResponse>.Failure(e.Message);
        }
    }
}
