using Application.Abstractions.Auth;
using Application.Abstractions.Common;
using Application.Abstractions.Import;
using Application.Abstractions.Pluggy;
using Doamain.Interface.Banks;
using Domain.interfaces;
using Domain.Interfaces;
using Application.Planning;
using Application.Planning.Flows;
using Domain.Interfaces.Card;
using Domain.Interfaces.Planning;
using Domain.Interfaces.Transactions;
using Domain.Services.Card;
using Domain.Services.Planning;
using Domain.Services.Planning.Rules;
using Infrastructure.Repositories.Card;
using Infrastructure.Repositories.Planning;
using Infra.Persistence.Repositories;
using Infra.Security;
using Infrastructure.AI;
using Infrastructure.AI.Transactions;
using Infrastructure.AI.Transactions.Tools;
using Infrastructure.Imports.Pluggy;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Banks;
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
        services.AddScoped<IBankRepository, BankRepository>();
        services.AddScoped<TransactionImportParserResolver>();

        services.AddScoped<ICardPurchaseRepository, CardPurchaseRepository>();
        services.AddScoped<ICardInvoiceRepository, CardInvoiceRepository>();
        services.AddScoped<ICreditCardBillingCalculator, CreditCardBillingCalculator>();

        services.AddScoped<IPlannedCommitmentRepository, PlannedCommitmentRepository>();

        services.AddScoped<IFinancialProjectionService, FinancialProjectionService>();
        services.AddScoped<IFinancialFlowSource, TransactionFlowSource>();
        services.AddScoped<IFinancialFlowSource, CardInvoiceFlowSource>();
        services.AddScoped<IFinancialFlowSource, CommitmentFlowSource>();
        services.AddScoped<IFinancialFlowSource, VariableSpendingFlowSource>();
        services.AddScoped<IFinancialFlowAggregator, FinancialFlowAggregator>();

        services.AddScoped<IVariableSpendingEstimator, VariableSpendingEstimator>();
        services.AddScoped<ISafetyReserveCalculator, SafetyReserveCalculator>();
        services.AddScoped<IViabilityRule, NegativeBalanceRule>();
        services.AddScoped<IViabilityRule, SafetyReserveRule>();
        services.AddScoped<IViabilityRule, ConfidenceRule>();
        services.AddScoped<IViabilityRule, DecliningBalanceRule>();
        services.AddScoped<IViabilityRule, LowBufferRule>();
        services.AddScoped<IViabilityEvaluator, ViabilityEvaluator>();

        services.AddScoped<IProjectionRunner, ProjectionRunner>();
        services.AddScoped<IPurchaseFlowBuilder, PurchaseFlowBuilder>();

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
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpClient<IPluggyService, PluggyService>();


        return services;
    }
}
