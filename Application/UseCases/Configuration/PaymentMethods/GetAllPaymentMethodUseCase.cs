using Application.Abstractions;
using Application.DTOs.Configuration.PaymentMethods;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.PaymentMethods;

public sealed class GetAllPaymentMethodUseCase
    : IUseCase<GetAllPaymentMethodsRequest, IEnumerable<PaymentMethodResponse>>
{
    private readonly IPaymentMethodRepository _repository;

    public GetAllPaymentMethodUseCase(IPaymentMethodRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResultEntity<IEnumerable<PaymentMethodResponse>>> ExecuteAsync(GetAllPaymentMethodsRequest request)
    {
        var categories = await _repository.GetAllAsync(request.UserId);

        var response = categories.Select(c =>
            new PaymentMethodResponse(
                c.Id,
                c.Description,
                c.Active
            ));

        return ResultEntity<IEnumerable<PaymentMethodResponse>>.Success(response, MessageKeys.OperationSuccess);
    }
}
