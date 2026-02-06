using Application.Abstractions;
using Application.DTOs.Configuration.PaymentMethods;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.PaymentMethods;

public sealed class GetPaymentMethodByIdUseCase
    : IUseCase<PaymentMethodIdRequest, PaymentMethodResponse>
{
    private readonly IPaymentMethodRepository _repository;

    public GetPaymentMethodByIdUseCase(IPaymentMethodRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResultT<PaymentMethodResponse>> ExecuteAsync(PaymentMethodIdRequest request)
    {
        var category = await _repository.GetByIdAsync(request.Id, request.UserId);

        if (category is null)
            return ResultT<PaymentMethodResponse>.Failure("", MessageKeys.PaymentMethodNotFound);

        return ResultT<PaymentMethodResponse>.Success(
            new PaymentMethodResponse(
                category.Id,
                category.Description,
                category.CodTypePaymentMethod,
                category.Active
            )
        );
    }
}
