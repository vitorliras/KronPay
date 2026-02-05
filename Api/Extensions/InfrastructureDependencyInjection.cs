using Application.Abstractions.Auth;
using Application.Executors;
using Application.Pipelines;
using Application.UseCases.Auth;
using Application.UseCases.Categories;
using Application.UseCases.PaymentMethods;
using Domain.interfaces;
using Domain.Interfaces;
using Infra.Persistence.Repositories;
using Infra.Security;
using Infrastructure.Context;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Api.Extensions;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")
            ));

        services.AddScoped<UseCaseExecutor>();
        services.AddScoped(typeof(ValidationPipeline<,>));

        #region Use Cases

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

        #endregion

        #region Repositories

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryItemRepository, CategoryItemRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<LoginUseCase>();

        #endregion

        return services;
    }
}
