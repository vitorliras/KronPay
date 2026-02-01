using Application.Abstractions.Auth;
using Application.DTOs.Auth;
using Application.DTOs.Users;
using Domain.interfaces;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Auth;

public sealed class LoginUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _uow;

    public LoginUseCase( IUserRepository userRepository,IPasswordHasher passwordHasher,ITokenService tokenService, IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _uow = uow;
    }

    public async Task<ResultT<LoginResponse>> ExecuteAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null)
            return ResultT<LoginResponse>.Failure(MessageKeys.UsernameOrPasswordIsIncorrect);

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return ResultT<LoginResponse>.Failure(MessageKeys.UsernameOrPasswordIsIncorrect); 

        var token = _tokenService.GenerateToken(user);

        user.RegisterAccess();

        var result = _userRepository.Update(user);
        if (!result)
            return ResultT<LoginResponse>.Failure(MessageKeys.OperationFailed);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultT<LoginResponse>.Failure(MessageKeys.OperationFailed);

        return ResultT<LoginResponse>.Success(new LoginResponse
        {
            AccessToken = token,
            ExpiresAt = _tokenService.GetExpiration()
        }, MessageKeys.InvalidEmail);
    }
}
