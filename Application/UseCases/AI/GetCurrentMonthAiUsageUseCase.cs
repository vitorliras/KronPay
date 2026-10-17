using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.AI;
using Domain.Interfaces.AI;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.AI;

public sealed class GetCurrentMonthAiUsageUseCase : IUseCaseWithoutRequest<AiUsageSummaryResponse>
{
    private readonly IAiUsageLogRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetCurrentMonthAiUsageUseCase(
        IAiUsageLogRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<AiUsageSummaryResponse>> ExecuteAsync()
    {
        var userId = _currentUser.UserId;

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var logs = await _repository.GetByPeriodAsync(userId, monthStart, monthStart.AddMonths(1));

        var byFeature = logs
            .GroupBy(l => l.Feature)
            .Select(g => new AiUsageByFeatureResponse(
                g.Key,
                g.Count(),
                g.Sum(l => (long)l.InputTokens),
                g.Sum(l => (long)l.CachedInputTokens),
                g.Sum(l => (long)l.OutputTokens),
                g.Sum(l => l.EstimatedCostUsd)))
            .OrderByDescending(f => f.EstimatedCostUsd)
            .ToList();

        var response = new AiUsageSummaryResponse(
            now.Year,
            now.Month,
            logs.Count,
            logs.Sum(l => (long)l.InputTokens),
            logs.Sum(l => (long)l.CachedInputTokens),
            logs.Sum(l => (long)l.OutputTokens),
            logs.Sum(l => l.EstimatedCostUsd),
            byFeature);

        return ResultEntity<AiUsageSummaryResponse>.Success(response, MessageKeys.OperationSuccess);
    }
}
