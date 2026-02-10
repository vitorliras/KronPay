using Application.Abstractions;
using Application.DTOs.Configuration.PaymentMethods;
using Domain.Entities.Configuration;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.PaymentMethods;

public sealed class CreatePaymentMethodUseCase
    : IUseCase<CreatePaymentMethodRequest, PaymentMethodResponse>
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IUnitOfWork _uow;

    public CreatePaymentMethodUseCase(IPaymentMethodRepository paymentMethodrepository, IUnitOfWork uow)
    {
        _paymentMethodRepository = paymentMethodrepository;
        _uow = uow;
    }

    public async Task<ResultT<PaymentMethodResponse>> ExecuteAsync(CreatePaymentMethodRequest request)
    {
        var paymentMethod = await _paymentMethodRepository.GetByDescriptionAsync(request.Description, request.UserId);

        if (paymentMethod is not null)
            return ResultT<PaymentMethodResponse>.Failure("", MessageKeys.DescriptionAlreadyExists);

         paymentMethod = new PaymentMethod(
            request.UserId,
            request.Description
        );
        
        var result = await _paymentMethodRepository.AddAsync(paymentMethod);
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
