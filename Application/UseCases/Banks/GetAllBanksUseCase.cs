using Application.Abstractions;
using Application.Abstractions.Common;
using Doamain.Interface.Banks;
using Domain.Entities.banks;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.CreditCards;

public sealed class GetAllBanksUseCase
    : IUseCaseWithoutRequest<IEnumerable<Bank>>
{
    private readonly IBankRepository _repository;

    public GetAllBanksUseCase(IBankRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResultEntity<IEnumerable<Bank>>> ExecuteAsync()
    {
        var response = await _repository.GetAllAsync();

        return ResultEntity<IEnumerable<Bank>>.Success(response, MessageKeys.OperationSuccess);
    }
}
