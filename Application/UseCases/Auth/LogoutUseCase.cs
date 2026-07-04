using Application.Abstractions;
using Application.Abstractions.Auth;
using Application.DTOs.Auth;
using Domain.Interfaces;
using Domain.Interfaces.Auth;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Auth;

public sealed class LogoutUseCase
    : IUseCase<LogoutRequest, LogoutResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _uow;

    public LogoutUseCase(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IUnitOfWork uow)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _uow = uow;
    }

    public async Task<ResultEntity<LogoutResponse>> ExecuteAsync(LogoutRequest request)
    {
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (storedToken is null || storedToken.IsRevoked)
            return ResultEntity<LogoutResponse>.Success(new LogoutResponse(true), MessageKeys.OperationSuccess);

        storedToken.Revoke();
        _refreshTokenRepository.Update(storedToken);

        if (!await _uow.CommitAsync())
            return ResultEntity<LogoutResponse>.Failure(MessageKeys.DataPersistenceFailed);

        return ResultEntity<LogoutResponse>.Success(new LogoutResponse(true), MessageKeys.OperationSuccess);
    }
}
