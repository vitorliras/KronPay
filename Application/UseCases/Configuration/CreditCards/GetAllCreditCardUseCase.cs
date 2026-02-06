using Application.Abstractions;
using Application.DTOs.Configuration.CreditCards;
using Domain.Interfaces;
using Shared.Results;

namespace Application.UseCases.CreditCards;

public sealed class GetAllCreditCardUseCase
    : IUseCase<GetAllCreditCardsRequest, IEnumerable<CreditCardResponse>>
{
    private readonly ICreditCardRepository _repository;

    public GetAllCreditCardUseCase(ICreditCardRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResultT<IEnumerable<CreditCardResponse>>> ExecuteAsync(GetAllCreditCardsRequest request)
    {
        var creditCards = await _repository.GetAllAsync(request.UserId);

        var response = creditCards.Select(creditCard =>
            new CreditCardResponse(
                creditCard.Id,
                creditCard.Description,
                creditCard.DueDay,
                creditCard.ClosingDay,
                creditCard.CreditLimit,
                creditCard.Active
            ));

        return ResultT<IEnumerable<CreditCardResponse>>.Success(response);
    }
}
