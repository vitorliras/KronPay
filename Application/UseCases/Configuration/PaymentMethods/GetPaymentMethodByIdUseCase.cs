using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Configuration.PaymentMethods;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.PaymentMethods;

public sealed class GetPaymentMethodByIdUseCase
    : IUseCase<PaymentMethodIdRequest, PaymentMethodResponse>
{
    private readonly IPaymentMethodRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetPaymentMethodByIdUseCase(IPaymentMethodRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser; 
    }

    public async Task<ResultEntity<PaymentMethodResponse>> ExecuteAsync(PaymentMethodIdRequest request)
    {
        var userId = _currentUser.UserId;

        var category = await _repository.GetByIdAsync(request.Id, userId);

        if (category is null)
            return ResultEntity<PaymentMethodResponse>.Failure("", MessageKeys.PaymentMethodNotFound);

        return ResultEntity<PaymentMethodResponse>.Success(
            new PaymentMethodResponse(
                category.Id,
                category.Description,
                category.Active
            ), MessageKeys.OperationSuccess
        );
    }
}
