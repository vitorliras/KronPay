using Application.Abstractions;
using Application.DTOs.Configuration.PaymentMethods;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.PaymentMethods;

public sealed class UpdatePaymentMethodUseCase
    : IUseCase<UpdatePaymentMethodRequest, PaymentMethodResponse>
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IUnitOfWork _uow;

    public UpdatePaymentMethodUseCase(IPaymentMethodRepository paymentMethodRepository, IUnitOfWork uow)
    {
        _paymentMethodRepository = paymentMethodRepository;
        _uow = uow;
    }

    public async Task<ResultT<PaymentMethodResponse>> ExecuteAsync(UpdatePaymentMethodRequest request)
    {
        var paymentMethod = await _paymentMethodRepository.GetByDescriptionAsync(request.Description, request.UserId);

        if (paymentMethod is not null)
            return ResultT<PaymentMethodResponse>.Failure("", MessageKeys.DescriptionAlreadyExists);

         paymentMethod = await _paymentMethodRepository.GetByIdAsync(request.Id, request.UserId);

        if (paymentMethod is null)
            return ResultT<PaymentMethodResponse>.Failure("", MessageKeys.PaymentMethodNotFound);

        paymentMethod.UpdateDescription(request.Description);

        var result =  _paymentMethodRepository.Update(paymentMethod);
        if (!result)
            return ResultT<PaymentMethodResponse>.Failure("", MessageKeys.OperationFailed);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultT<PaymentMethodResponse>.Failure("", MessageKeys.OperationFailed);

        return ResultT<PaymentMethodResponse>.Success(
            new PaymentMethodResponse(
                paymentMethod.Id,
                paymentMethod.Description,
                paymentMethod.Active
            )
        );
    }
}
