using Application.Abstractions;
using Application.Abstractions.Common;
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
    private readonly ICurrentUserService _currentUser;

    public GetCreditCardByIdUseCase(ICreditCardRepository repository, ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<CreditCardResponse>> ExecuteAsync(CreditCardIdRequest request)
    {
        var userId = _currentUser.UserId;

        var creditCard = await _repository.GetByIdAsync(request.Id, userId);

        if (creditCard is null)
            return ResultEntity<CreditCardResponse>.Failure("", MessageKeys.CreditCardNotFound);

        return ResultEntity<CreditCardResponse>.Success(
            new CreditCardResponse(
                creditCard.Id,
                creditCard.Description,
                creditCard.DueDay,
                creditCard.ClosingDay,
                creditCard.CreditLimit,
                creditCard.Active
            ), MessageKeys.OperationSuccess
        );
    }
}
