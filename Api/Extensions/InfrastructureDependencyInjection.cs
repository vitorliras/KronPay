using Application.Abstractions.Auth;
using Application.Abstractions.Import;
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
using Infrastructure.AI;
using Infrastructure.AI.Transactions;
using Infrastructure.AI.Transactions.Tools;
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
        services.AddScoped<ITransactionImportParser, CsvTransactionImportParser>();
        services.AddScoped<ITransactionImportParser, OfxTransactionImportParser>();
        services.AddScoped<TransactionImportParserResolver>();

        #endregion

        services.AddHttpClient<OllamaTransactionClassifier>(client =>
        {
            client.BaseAddress = new Uri(configuration["Ollama:BaseUrl"]!);
        });

        services.AddHttpClient<OllamaBatchTransactionClassifier>(client =>
        {
            client.BaseAddress = new Uri(configuration["Ollama:BaseUrl"]!);
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        services.AddScoped<OpenAiTransactionClassifier>();
        services.AddScoped<GroqBatchTransactionClassifier>();
        services.AddScoped<FallbackTransactionClassifier>();
        services.AddScoped<OpenAiClient>();

        services.AddScoped<ITransactionAiClassifier, OpenAiTransactionClassifier>();
        services.AddScoped<ITransactionAiBatchClassifier, TransactionAiClassifier>();





        return services;
    }
}
