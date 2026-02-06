using Application.Abstractions;
using Application.DTOs.Configuration.CreditCards;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.CreditCards;

public sealed class DeactivateCreditCardUseCase
    : IUseCase<CreditCardIdRequest, Unit>
{
    private readonly ICreditCardRepository _paymentMethodRepository;
    private readonly IUnitOfWork _uow;

    public DeactivateCreditCardUseCase(ICreditCardRepository paymentMethodRepository, IUnitOfWork uow)
    {
        _paymentMethodRepository = paymentMethodRepository;
        _uow = uow;
    }
    public async Task<ResultT<Unit>> ExecuteAsync(CreditCardIdRequest request)
    {
        var paymentMethodItem = await _paymentMethodRepository.GetByIdAsync(request.Id, request.UserId);
        if (paymentMethodItem is null)
            return ResultT<Unit>.Failure("", MessageKeys.CreditCardNotFound);

        paymentMethodItem.Deactivate();
        var result = _paymentMethodRepository.Update(paymentMethodItem);
        if (!result)
            return ResultT<Unit>.Failure("", MessageKeys.OperationFailed);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultT<Unit>.Failure("", MessageKeys.OperationFailed);

        return ResultT<Unit>.Success(Unit.Value);
    }
}
