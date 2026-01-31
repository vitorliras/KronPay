using Application.Abstractions.Auth;
using Application.DTOs.Auth;
using Domain.interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Auth;

public sealed class LoginUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;


    public LoginUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
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

        await _userRepository.UpdateAsync(user);

        return ResultT<LoginResponse>.Success(new LoginResponse
        {
            AccessToken = token,
            ExpiresAt = _tokenService.GetExpiration()
        }, MessageKeys.InvalidEmail);
    }
}
