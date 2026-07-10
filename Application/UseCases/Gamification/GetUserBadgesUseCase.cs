using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Gamification;
using Application.Gamification;
using Domain.Interfaces.Gamification;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Gamification;

public sealed class GetUserBadgesUseCase : IUseCaseWithoutRequest<IReadOnlyList<UserBadgeResponse>>
{
    private readonly IUserBadgeRepository _badgeRepository;
    private readonly ICurrentUserService _currentUser;

    public GetUserBadgesUseCase(
        IUserBadgeRepository badgeRepository,
        ICurrentUserService currentUser)
    {
        _badgeRepository = badgeRepository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<IReadOnlyList<UserBadgeResponse>>> ExecuteAsync()
    {
        var userId = _currentUser.UserId;

        var unlockedBadges = await _badgeRepository.GetByUserIdAsync(userId);
        var unlockedByCode = unlockedBadges.ToDictionary(x => x.Code, x => x.UnlockedAt);

        var response = BadgeCatalog.All
            .Select(entry =>
            {
                var isUnlocked = unlockedByCode.TryGetValue(entry.Key, out var unlockedAt);
                return new UserBadgeResponse(
                    entry.Key.ToString(),
                    entry.Value.Tier.ToString(),
                    entry.Value.MessageKey,
                    entry.Value.DescriptionMessageKey,
                    isUnlocked,
                    isUnlocked ? unlockedAt : null);
            })
            .ToList();

        return ResultEntity<IReadOnlyList<UserBadgeResponse>>.Success(response, MessageKeys.OperationSuccess);
    }
}
