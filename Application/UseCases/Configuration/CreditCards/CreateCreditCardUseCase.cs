using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Configuration.CreditCards;
using Domain.Entities;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.creditCards;

public sealed class CreateCreditCardUseCase
    : IUseCase<CreateCreditCardRequest, CreditCardResponse>
{
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateCreditCardUseCase(ICreditCardRepository creditCardrepository, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _creditCardRepository = creditCardrepository;
        _uow = uow;
        _currentUser = currentUser;

    }

    public async Task<ResultEntity<CreditCardResponse>> ExecuteAsync(CreateCreditCardRequest request)
    {
        var userId = _currentUser.UserId;

        var creditCard = await _creditCardRepository.GetByDescriptionAsync(request.Description, userId);

        if (creditCard is not null)
            return ResultEntity<CreditCardResponse>.Failure("", MessageKeys.DescriptionAlreadyExists);

         creditCard = new CreditCard(
            0,
            userId,
            request.Description,
            request.DueDay,
            request.ClosingDay,
            request.CreditLimit
        );


        var result = await _creditCardRepository.AddAsync(creditCard);
        if (!result)
            return ResultEntity<CreditCardResponse>.Failure("", MessageKeys.OperationFailed);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultEntity<CreditCardResponse>.Failure("", MessageKeys.OperationFailed);

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
