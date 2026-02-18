using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Configuration.CreditCards;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.CreditCards;

public sealed class GetAllCreditCardUseCase
    : IUseCaseWithoutRequest<IEnumerable<CreditCardResponse>>
{
    private readonly ICreditCardRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetAllCreditCardUseCase(ICreditCardRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<IEnumerable<CreditCardResponse>>> ExecuteAsync()
    {
        var userId = _currentUser.UserId;

        var creditCards = await _repository.GetAllAsync(userId);

        var response = creditCards.Select(creditCard =>
            new CreditCardResponse(
                creditCard.Id,
                creditCard.Description,
                creditCard.DueDay,
                creditCard.ClosingDay,
                creditCard.CreditLimit,
                creditCard.Active
            ));

        return ResultEntity<IEnumerable<CreditCardResponse>>.Success(response, MessageKeys.OperationSuccess);
    }
}
