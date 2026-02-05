using Application.Abstractions;
using Application.DTOs.Configuration.PaymentMethods;
using Domain.Interfaces;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.PaymentMethods;

public sealed class UpdatePaymentMethodUseCase
    : IUseCase<UpdatePaymentMethodRequest, PaymentMethodResponse>
{
    private readonly IPaymentMethodRepository _categoryRepository;
    private readonly IUnitOfWork _uow;

    public UpdatePaymentMethodUseCase(IPaymentMethodRepository categoryRepository, IUnitOfWork uow)
    {
        _categoryRepository = categoryRepository;
        _uow = uow;
    }

    public async Task<ResultT<PaymentMethodResponse>> ExecuteAsync(UpdatePaymentMethodRequest request)
    {
        var category = await _categoryRepository.GetByDescriptionAsync(request.Description, request.UserId);

        if (category is not null)
            return ResultT<PaymentMethodResponse>.Failure("", MessageKeys.DescriptionAlreadyExists);

         category = await _categoryRepository.GetByIdAsync(request.Id, request.UserId);

        if (category is null)
            return ResultT<PaymentMethodResponse>.Failure("", MessageKeys.PaymentMethodNotFound);

        category.UpdateDescription(request.Description);

        var result =  _categoryRepository.Update(category);
        if (!result)
            return ResultT<PaymentMethodResponse>.Failure("", MessageKeys.OperationFailed);

        var uow = await _uow.CommitAsync();
        if (!uow)
            return ResultT<PaymentMethodResponse>.Failure("", MessageKeys.OperationFailed);

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
