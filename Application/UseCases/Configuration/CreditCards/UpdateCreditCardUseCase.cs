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

    public async Task<ResultEntity<CreditCardResponse>> ExecuteAsync(UpdateCreditCardRequest request)
    {
        var creditCard = await _creditCardRepository.GetByDescriptionAsync(request.Description, request.UserId);

        if (creditCard is not null)
            return ResultEntity<CreditCardResponse>.Failure("", MessageKeys.DescriptionAlreadyExists);

         creditCard = await _creditCardRepository.GetByIdAsync(request.Id, request.UserId);

        if (creditCard is null)
            return ResultEntity<CreditCardResponse>.Failure("", MessageKeys.CreditCardNotFound);

        creditCard.UpdateDescription(request.Description);

        var result =  _creditCardRepository.Update(creditCard);
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
