using Application.Abstractions;
using Application.Abstractions.Common;
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
    private readonly ICurrentUserService _currentUser;

    public UpdatePaymentMethodUseCase(IPaymentMethodRepository paymentMethodRepository, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _paymentMethodRepository = paymentMethodRepository;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<PaymentMethodResponse>> ExecuteAsync(UpdatePaymentMethodRequest request)
    {
        var userId = _currentUser.UserId;

        var paymentMethod = await _paymentMethodRepository.GetByDescriptionAsync(request.Description, userId);

        if (paymentMethod is not null)
            return ResultEntity<PaymentMethodResponse>.Failure("", MessageKeys.DescriptionAlreadyExists);

         paymentMethod = await _paymentMethodRepository.GetByIdAsync(request.Id, userId);

        if (paymentMethod is null)
            return ResultEntity<PaymentMethodResponse>.Failure("", MessageKeys.PaymentMethodNotFound);

        paymentMethod.UpdateDescription(request.Description);

        var result =  _paymentMethodRepository.Update(paymentMethod);
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
