using Application.Abstractions;
using Application.Abstractions.Auth;
using Application.DTOs.Auth;
using Domain.Entities.Auth;
using Domain.interfaces;
using Domain.Interfaces;
using Domain.Interfaces.Auth;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Auth;

public sealed class RefreshTokenUseCase
    : IUseCase<RefreshTokenRequest, RefreshTokenResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _uow;

    public RefreshTokenUseCase(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        ITokenService tokenService,
        IUnitOfWork uow)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _uow = uow;
    }

    public async Task<ResultEntity<RefreshTokenResponse>> ExecuteAsync(RefreshTokenRequest request)
    {
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (storedToken is null || storedToken.IsRevoked)
            return ResultEntity<RefreshTokenResponse>.Failure(MessageKeys.InvalidRefreshToken);

        var now = DateTime.UtcNow;
        if (storedToken.IsExpired(now))
            return ResultEntity<RefreshTokenResponse>.Failure(MessageKeys.RefreshTokenExpired);

        var user = await _userRepository.GetByIdAsync(storedToken.UserId);
        if (user is null)
            return ResultEntity<RefreshTokenResponse>.Failure(MessageKeys.InvaldUser);

        storedToken.Revoke();
        _refreshTokenRepository.Update(storedToken);

        var tokenResult = _tokenService.GenerateToken(user);
        var newRefreshTokenResult = _tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = _tokenService.HashRefreshToken(newRefreshTokenResult.Token);

        var newRefreshToken = new RefreshToken(user.Id, newRefreshTokenHash, newRefreshTokenResult.ExpiresAt);

        var added = await _refreshTokenRepository.AddAsync(newRefreshToken);
        if (!added)
            return ResultEntity<RefreshTokenResponse>.Failure(MessageKeys.InsertFalied);

        if (!await _uow.CommitAsync())
            return ResultEntity<RefreshTokenResponse>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<RefreshTokenResponse>.Success(
            new RefreshTokenResponse(tokenResult.Token, tokenResult.ExpiresAt, newRefreshTokenResult.Token),
            MessageKeys.OperationSuccess);
    }
}
