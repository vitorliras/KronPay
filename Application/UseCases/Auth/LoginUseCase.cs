using Application.Abstractions.Auth;
using Application.DTOs.Auth;
using Application.DTOs.Users;
using Domain.Entities.Auth;
using Domain.interfaces;
using Domain.Interfaces;
using Domain.Interfaces.Auth;
using KronPay.Domain.Entities.Users;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Auth;

public sealed class LoginUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _uow;

    public LoginUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _uow = uow;
    }

    public async Task<ResultEntity<LoginResponse>> ExecuteAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null)
            return ResultEntity<LoginResponse>.Failure(MessageKeys.UsernameOrPasswordIsIncorrect);

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return ResultEntity<LoginResponse>.Failure(MessageKeys.UsernameOrPasswordIsIncorrect);

        if (!user.EmailConfirmed)
            return ResultEntity<LoginResponse>.Failure(MessageKeys.AccountNotConfirmed);

        var tokenResult = _tokenService.GenerateToken(user);

        string? refreshToken = null;
        if (request.RememberMe)
        {
            var refreshTokenResult = _tokenService.GenerateRefreshToken();
            var refreshTokenHash = _tokenService.HashRefreshToken(refreshTokenResult.Token);

            var refreshTokenEntity = new RefreshToken(user.Id, refreshTokenHash, refreshTokenResult.ExpiresAt);

            var refreshTokenAdded = await _refreshTokenRepository.AddAsync(refreshTokenEntity);
            if (!refreshTokenAdded)
                return ResultEntity<LoginResponse>.Failure(MessageKeys.InsertFalied);

            refreshToken = refreshTokenResult.Token;
        }

        user.RegisterAccess();

        var result = _userRepository.Update(user);
        if (!result)
            return ResultEntity<LoginResponse>.Failure(MessageKeys.UpdateFailed);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultEntity<LoginResponse>.Failure(MessageKeys.DataPersistenceFailed);

        var type = await _userRepository.GetTypeUserByCode(user.UserType);

        return ResultEntity<LoginResponse>.Success(new LoginResponse
        {
            AccessToken = tokenResult.Token,
            ExpiresAt = tokenResult.ExpiresAt,
            User = user.Username.Value,
            TypeUser = type.Description,
            RefreshToken = refreshToken
        }, MessageKeys.OperationSuccess);
    }
}
