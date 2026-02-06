using Application.Abstractions;
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

    public CreateCreditCardUseCase(ICreditCardRepository creditCardrepository, IUnitOfWork uow)
    {
        _creditCardRepository = creditCardrepository;
        _uow = uow;
    }

    public async Task<ResultT<CreditCardResponse>> ExecuteAsync(CreateCreditCardRequest request)
    {
        var creditCard = await _creditCardRepository.GetByDescriptionAsync(request.Description, request.UserId);

        if (creditCard is not null)
            return ResultT<CreditCardResponse>.Failure("", MessageKeys.DescriptionAlreadyExists);

         creditCard = new CreditCard(
            0,
            request.UserId,
            request.Description,
            request.DueDay,
            request.ClosingDay,
            request.CreditLimit
        );


        var result = await _creditCardRepository.AddAsync(creditCard);
        if (!result)
            return ResultT<CreditCardResponse>.Failure("", MessageKeys.OperationFailed);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultT<CreditCardResponse>.Failure("", MessageKeys.OperationFailed);

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
