using Application.Abstractions;
using Application.DTOs.Configuration.CreditCards;
using Domain.Entities;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.CreditCards;

public sealed class GetCreditCardByIdUseCase
    : IUseCase<CreditCardIdRequest, CreditCardResponse>
{
    private readonly ICreditCardRepository _repository;

    public GetCreditCardByIdUseCase(ICreditCardRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResultT<CreditCardResponse>> ExecuteAsync(CreditCardIdRequest request)
    {
        var creditCard = await _repository.GetByIdAsync(request.Id, request.UserId);

        if (creditCard is null)
            return ResultT<CreditCardResponse>.Failure("", MessageKeys.CreditCardNotFound);

        return ResultT<CreditCardResponse>.Success(
            new CreditCardResponse(
                creditCard.Id,
                creditCard.Description,
                creditCard.DueDay,
                creditCard.ClosingDay,
                creditCard.CreditLimit,
                creditCard.Active
            )
        );
    }
}
