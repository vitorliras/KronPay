using Application.Abstractions;
using Application.Abstractions.Common;
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
    private readonly ICurrentUserService _currentUser;


    public DeactivateCreditCardUseCase(ICreditCardRepository paymentMethodRepository, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _paymentMethodRepository = paymentMethodRepository;
        _uow = uow;
        _currentUser = currentUser;
    }
    public async Task<ResultEntity<Unit>> ExecuteAsync(CreditCardIdRequest request)
    {
        var userId = _currentUser.UserId;


        var paymentMethodItem = await _paymentMethodRepository.GetByIdAsync(request.Id, userId);
        if (paymentMethodItem is null)
            return ResultEntity<Unit>.Failure("", MessageKeys.CreditCardNotFound);

        paymentMethodItem.Deactivate();
        var result = _paymentMethodRepository.Update(paymentMethodItem);
        if (!result)
            return ResultEntity<Unit>.Failure("", MessageKeys.OperationFailed);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultEntity<Unit>.Failure("", MessageKeys.OperationFailed);

        return ResultEntity<Unit>.Success(Unit.Value, MessageKeys.OperationSuccess);
    }
}
