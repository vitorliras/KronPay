using Application.Abstractions;
using Application.DTOs.Configuration.CreditCards;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.CreditCards;

public sealed class UpdateCreditCardUseCase
    : IUseCase<UpdateCreditCardRequest, CreditCardResponse>
{
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly IUnitOfWork _uow;

    public UpdateCreditCardUseCase(ICreditCardRepository creditCardRepository, IUnitOfWork uow)
    {
        _creditCardRepository = creditCardRepository;
        _uow = uow;
    }

    public async Task<ResultT<CreditCardResponse>> ExecuteAsync(UpdateCreditCardRequest request)
    {
        var creditCard = await _creditCardRepository.GetByDescriptionAsync(request.Description, request.UserId);

        if (creditCard is not null)
            return ResultT<CreditCardResponse>.Failure("", MessageKeys.DescriptionAlreadyExists);

         creditCard = await _creditCardRepository.GetByIdAsync(request.Id, request.UserId);

        if (creditCard is null)
            return ResultT<CreditCardResponse>.Failure("", MessageKeys.CreditCardNotFound);

        creditCard.UpdateDescription(request.Description);

        var result =  _creditCardRepository.Update(creditCard);
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
