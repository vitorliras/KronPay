using Application.Abstractions;
using Application.Abstractions.Common;
using Application.DTOs.Configuration.PaymentMethods;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.PaymentMethods;

public sealed class GetAllPaymentMethodUseCase
    : IUseCaseWithoutRequest< IEnumerable<PaymentMethodResponse>>
{
    private readonly IPaymentMethodRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetAllPaymentMethodUseCase(IPaymentMethodRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ResultEntity<IEnumerable<PaymentMethodResponse>>> ExecuteAsync()
    {
        var userId = _currentUser.UserId;

        var categories = await _repository.GetAllAsync(userId);

        var response = categories.Select(c =>
            new PaymentMethodResponse(
                c.Id,
                c.Description,
                c.Active
            ));

        return ResultEntity<IEnumerable<PaymentMethodResponse>>.Success(response, MessageKeys.OperationSuccess);
    }
}
