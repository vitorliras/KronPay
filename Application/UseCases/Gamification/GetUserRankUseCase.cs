using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Gamification;
using Application.Gamification;
using Domain.Enums.Gamification;
using Domain.Interfaces.Gamification;
using Domain.Services.Gamification;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Gamification;

public sealed class GetUserRankUseCase : IUseCaseWithoutRequest<UserRankResponse>
{
    private const int ProximityMargin = 50;
    private const int RiskMargin = 30;

    private readonly IUserRankProfileRepository _rankRepository;
    private readonly IUserBadgeRepository _badgeRepository;
    private readonly IRankScoringCalculator _calculator;
    private readonly ICurrentUserService _currentUser;

    public GetUserRankUseCase(
        IUserRankProfileRepository rankRepository,
        IUserBadgeRepository badgeRepository,
        IRankScoringCalculator calculator,
        ICurrentUserService currentUser)
    {
        _rankRepository = rankRepository;
        _badgeRepository = badgeRepository;
        _calculator = calculator;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<UserRankResponse>> ExecuteAsync()
    {
        var userId = _currentUser.UserId;

        var profile = await _rankRepository.GetByUserIdAsync(userId);
        var tier = profile?.Tier ?? RankTier.Cobre;
        var score = profile?.Score ?? 0;

        var proximityMessageKey = CalculateProximityMessageKey(tier, score);

        var badges = await _badgeRepository.GetByUserIdAsync(userId);
        var bronzeCount = 0;
        var prataCount = 0;
        var ouroCount = 0;

        foreach (var badge in badges)
        {
            var catalogEntry = BadgeCatalog.Get(badge.Code);
            switch (catalogEntry.Tier)
            {
                case BadgeTier.Bronze:
                    bronzeCount++;
                    break;
                case BadgeTier.Prata:
                    prataCount++;
                    break;
                case BadgeTier.Ouro:
                    ouroCount++;
                    break;
            }
        }

        var response = new UserRankResponse(
            tier.ToString(),
            proximityMessageKey,
            bronzeCount,
            prataCount,
            ouroCount);

        return ResultEntity<UserRankResponse>.Success(response, MessageKeys.OperationSuccess);
    }

    private string CalculateProximityMessageKey(RankTier tier, int score)
    {
        var nextTier = _calculator.GetNextTier(tier);
        if (nextTier.HasValue && _calculator.GetTierMinimumScore(nextTier.Value) - score <= ProximityMargin)
            return MessageKeys.RankProximityCloseToPromotion;

        var currentMin = _calculator.GetTierMinimumScore(tier);
        if (tier != RankTier.Cobre && score - currentMin <= RiskMargin)
            return MessageKeys.RankProximityAtRisk;

        return MessageKeys.RankProximityStable;
    }
}
