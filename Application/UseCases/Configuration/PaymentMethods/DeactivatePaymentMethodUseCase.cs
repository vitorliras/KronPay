using Application.Abstractions;
using Application.DTOs.Configuration.PaymentMethods;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.PaymentMethods;

public sealed class DeactivatePaymentMethodUseCase
    : IUseCase<PaymentMethodIdRequest, Unit>
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IUnitOfWork _uow;

    public DeactivatePaymentMethodUseCase(IPaymentMethodRepository paymentMethodRepository, IUnitOfWork uow)
    {
        _paymentMethodRepository = paymentMethodRepository;
        _uow = uow;
    }
    public async Task<ResultT<Unit>> ExecuteAsync(PaymentMethodIdRequest request)
    {
        var paymentMethodItem = await _paymentMethodRepository.GetByIdAsync(request.Id, request.UserId);
        if (paymentMethodItem is null)
            return ResultT<Unit>.Failure("", MessageKeys.PaymentMethodNotFound);

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
