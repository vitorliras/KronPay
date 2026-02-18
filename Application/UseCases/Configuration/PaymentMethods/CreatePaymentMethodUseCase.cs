using Application.Abstractions;
using Application.Abstractions.Common;
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
    private readonly ICurrentUserService _currentUser;

    public CreatePaymentMethodUseCase(IPaymentMethodRepository paymentMethodrepository, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _paymentMethodRepository = paymentMethodrepository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<PaymentMethodResponse>> ExecuteAsync(CreatePaymentMethodRequest request)
    {
        var userId = _currentUser.UserId;

        var paymentMethod = await _paymentMethodRepository.GetByDescriptionAsync(request.Description, userId);

        if (paymentMethod is not null)
            return ResultEntity<PaymentMethodResponse>.Failure("", MessageKeys.DescriptionAlreadyExists);

         paymentMethod = new PaymentMethod(
            userId,
            request.Description
        );
        
        var result = await _paymentMethodRepository.AddAsync(paymentMethod);
        if (!result)
            return ResultEntity<PaymentMethodResponse>.Failure("", MessageKeys.OperationFailed);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultEntity<PaymentMethodResponse>.Failure("", MessageKeys.OperationFailed);

        return ResultEntity<PaymentMethodResponse>.Success(
            new PaymentMethodResponse(
                paymentMethod.Id,
                paymentMethod.Description,
                paymentMethod.Active
            ), MessageKeys.OperationSuccess
        );
    }
}
