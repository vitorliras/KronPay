using Application.Abstractions;
using Application.Abstractions.AI;
using Application.DTOs.AI;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.AI;

public sealed class GetAiStatusUseCase : IUseCaseWithoutRequest<AiStatusResponse>
{
    private readonly IAiAvailability _availability;

    public GetAiStatusUseCase(IAiAvailability availability)
    {
        _availability = availability;
    }

    public Task<ResultEntity<AiStatusResponse>> ExecuteAsync()
    {
        var active = _availability.GetActive();

        var response = active is null
            ? new AiStatusResponse(false, null, null, false)
            : new AiStatusResponse(true, active.Name, active.Settings.Model, active.Settings.SupportsFiles);

        return Task.FromResult(
            ResultEntity<AiStatusResponse>.Success(response, MessageKeys.OperationSuccess));
    }
}
