using Application.DTOs.Transactions;
using Application.Executors;
using Application.Pipelines;
using Application.UseCases.Auth;
using Application.UseCases.Categories;
using Application.UseCases.creditCards;
using Application.UseCases.CreditCards;
using Application.UseCases.PaymentMethods;
using Application.UseCases.Transactions;
using Application.UseCases.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<LoginUseCase>();
        services.AddScoped<UseCaseExecutor>();
        services.AddScoped<CreateUserUseCase>();
        services.AddScoped(typeof(ValidationPipeline<,>));

        services.AddScoped<CreateCategoryUseCase>();
        services.AddScoped<UpdateCategoryUseCase>();
        services.AddScoped<DeactivateCategoryUseCase>();
        services.AddScoped<GetAllCategoriesUseCase>();
        services.AddScoped<GetCategoryByIdUseCase>();

        services.AddScoped<CreateCategoryItemUseCase>();
        services.AddScoped<UpdateCategoryItemUseCase>();
        services.AddScoped<DeactivateCategoryItemUseCase>();
        services.AddScoped<GetAllCategoryItemsUseCase>();
        services.AddScoped<GetCategoryItemByIdUseCase>();

        services.AddScoped<CreatePaymentMethodUseCase>();
        services.AddScoped<UpdatePaymentMethodUseCase>();
        services.AddScoped<DeactivatePaymentMethodUseCase>();
        services.AddScoped<GetAllPaymentMethodUseCase>();
        services.AddScoped<GetPaymentMethodByIdUseCase>();

        services.AddScoped<CreateCreditCardUseCase>();
        services.AddScoped<UpdateCreditCardUseCase>();
        services.AddScoped<DeactivateCreditCardUseCase>();
        services.AddScoped<GetAllCreditCardUseCase>();
        services.AddScoped<GetCreditCardByIdUseCase>();

        services.AddScoped<CreateTransactionUseCase>();
        services.AddScoped<UpdateTransactionUseCase>();
        services.AddScoped<ChangeStatusTransactionUseCase>();
        services.AddScoped<DeleteTransactionUseCase>();
        services.AddScoped<GetTransactionsByIdGroupUseCase>();
        services.AddScoped<GetTransactionsByMonthUseCase>();
        services.AddScoped<GetTransactionsByYearUseCase>();

        return services;
    }
}
