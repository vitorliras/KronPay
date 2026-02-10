using Application.Abstractions.Auth;
using Application.Executors;
using Application.Pipelines;
using Application.UseCases.Auth;
using Application.UseCases.Categories;
using Application.UseCases.creditCards;
using Application.UseCases.CreditCards;
using Application.UseCases.PaymentMethods;
using Domain.interfaces;
using Domain.Interfaces;
using Domain.Interfaces.Transactions;
using Infra.Persistence.Repositories;
using Infra.Security;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Transactions;
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

        #region Repositories

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryItemRepository, CategoryItemRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<ICreditCardRepository, CreditCardRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransactionGroupRepository, TransactionGroupRepository>();

        #endregion

        return services;
    }
}
