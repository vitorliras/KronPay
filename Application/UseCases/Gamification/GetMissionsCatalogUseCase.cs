using Application.Abstractions;
using Application.DTOs.Gamification;
using Application.Gamification;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.Gamification;

public sealed class GetMissionsCatalogUseCase : IUseCaseWithoutRequest<IReadOnlyList<MissionCatalogResponse>>
{
    public Task<ResultEntity<IReadOnlyList<MissionCatalogResponse>>> ExecuteAsync()
    {
        var response = MissionCatalog.All
            .Select(entry => new MissionCatalogResponse(
                entry.Key.ToString(),
                entry.Value.Area,
                entry.Value.Significance.ToString(),
                entry.Value.MessageKey))
            .ToList();

        return Task.FromResult(
            ResultEntity<IReadOnlyList<MissionCatalogResponse>>.Success(response, MessageKeys.OperationSuccess));
    }
}
