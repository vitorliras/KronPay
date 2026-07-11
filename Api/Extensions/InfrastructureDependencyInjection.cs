using Application.Abstractions.Auth;
using Application.Abstractions.Common;
using Application.Abstractions.Email;
using Application.Abstractions.Import;
using Application.Abstractions.Pluggy;
using Application.DataRetention;
using Application.DataRetention.Targets;
using Application.Goals;
using Application.Notifications;
using Application.Notifications.Rules;
using Application.Services.Gamification;
using Application.Services.Gamification.Evaluators;
using Doamain.Interface.Banks;
using Domain.interfaces;
using Domain.Interfaces;
using Domain.Interfaces.Auth;
using Application.Planning;
using Application.Planning.Flows;
using Domain.Interfaces.Card;
using Domain.Interfaces.DataRetention;
using Domain.Interfaces.Gamification;
using Domain.Interfaces.Goals;
using Domain.Interfaces.Notifications;
using Domain.Interfaces.Planning;
using Domain.Interfaces.Transactions;
using Domain.Interfaces.Users;
using Domain.Services.Assistant;
using Domain.Services.Auth;
using Domain.Services.Card;
using Domain.Services.Gamification;
using Domain.Services.Goals;
using Domain.Services.Planning;
using Domain.Services.Planning.Rules;
using Domain.Services.Users;
using Infrastructure.Email;
using Infrastructure.Media;
using Infrastructure.Services.Assistant;
using Infrastructure.Repositories.Auth;
using Infrastructure.Repositories.Card;
using Infrastructure.Repositories.DataRetention;
using Infrastructure.Repositories.Gamification;
using Infrastructure.Repositories.Goals;
using Infrastructure.Repositories.Notifications;
using Infrastructure.Repositories.Planning;
using Infrastructure.Repositories.Users;
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
using Infrastructure.Workers;
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

        services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();
        services.AddScoped<IVerificationCodeService, VerificationCodeService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IUserProfilePhotoRepository, UserProfilePhotoRepository>();
        services.AddScoped<IProfilePhotoProcessor, ProfilePhotoProcessor>();

        services.AddScoped<IFinancialGoalRepository, FinancialGoalRepository>();
        services.AddScoped<ICategoryBudgetGoalRepository, CategoryBudgetGoalRepository>();
        services.AddScoped<IGoalContributionCalculator, GoalContributionCalculator>();

        services.AddScoped<IUserRankProfileRepository, UserRankProfileRepository>();
        services.AddScoped<IUserBadgeRepository, UserBadgeRepository>();
        services.AddScoped<IPointLedgerRepository, PointLedgerRepository>();
        services.AddScoped<IMissionStateSnapshotRepository, MissionStateSnapshotRepository>();
        services.AddScoped<IConsistencyCounterRepository, ConsistencyCounterRepository>();
        services.AddScoped<IGamificationEvaluationRunRepository, GamificationEvaluationRunRepository>();
        services.AddScoped<IRankScoringCalculator, RankScoringCalculator>();

        services.AddScoped<BudgetMissionEvaluator>();
        services.AddScoped<CardMissionEvaluator>();
        services.AddScoped<GoalMissionEvaluator>();
        services.AddScoped<PlanningMissionEvaluator>();
        services.AddScoped<SpendingTrendMissionEvaluator>();
        services.AddScoped<NotificationMissionEvaluator>();
        services.AddScoped<TransactionMissionEvaluator>();
        services.AddScoped<BadgeUnlockEvaluator>();
        services.AddScoped<GamificationMissionApplier>();
        services.AddScoped<IGamificationEvaluationOrchestrator, GamificationEvaluationOrchestrator>();
        services.AddScoped<IGamificationService, GamificationService>();
        services.AddHostedService<GamificationEvaluationWorker>();

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>();
        services.AddScoped<INotificationEvaluationRunRepository, NotificationEvaluationRunRepository>();

        services.AddScoped<ISpendingTrendCalculator, SpendingTrendCalculator>();
        services.AddScoped<IGoalRiskEvaluator, GoalRiskEvaluator>();

        services.AddScoped<ITransactionNotificationRuleEvaluator, TransactionNotificationRuleEvaluator>();
        services.AddScoped<ICardInvoiceNotificationRuleEvaluator, CardInvoiceNotificationRuleEvaluator>();
        services.AddScoped<IGoalNotificationRuleEvaluator, GoalNotificationRuleEvaluator>();
        services.AddScoped<IFinancialIntelligenceNotificationRuleEvaluator, FinancialIntelligenceNotificationRuleEvaluator>();
        services.AddScoped<IDataHygieneNotificationRuleEvaluator, DataHygieneNotificationRuleEvaluator>();
        services.AddScoped<INotificationEmailDispatcher, NotificationEmailDispatcher>();
        services.AddScoped<INotificationEvaluationOrchestrator, NotificationEvaluationOrchestrator>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddHostedService<NotificationEvaluationWorker>();

        services.AddScoped<IDataRetentionPurgeRunRepository, DataRetentionPurgeRunRepository>();
        services.AddScoped<IRetentionPurgeTarget, CategoryRetentionPurgeTarget>();
        services.AddScoped<IRetentionPurgeTarget, CategoryItemRetentionPurgeTarget>();
        services.AddScoped<IRetentionPurgeTarget, PaymentMethodRetentionPurgeTarget>();
        services.AddScoped<IRetentionPurgeTarget, CreditCardRetentionPurgeTarget>();
        services.AddScoped<IRetentionPurgeTarget, CardPurchaseRetentionPurgeTarget>();
        services.AddScoped<IRetentionPurgeTarget, CategoryBudgetGoalRetentionPurgeTarget>();
        services.AddScoped<IRetentionPurgeTarget, PlannedCommitmentRetentionPurgeTarget>();
        services.AddScoped<IDataRetentionPurgeOrchestrator, DataRetentionPurgeOrchestrator>();
        services.AddHostedService<DataRetentionPurgeWorker>();

        services.AddScoped<IFinancialProjectionService, FinancialProjectionService>();
        services.AddScoped<IFinancialFlowSource, TransactionFlowSource>();
        services.AddScoped<IFinancialFlowSource, CardInvoiceFlowSource>();
        services.AddScoped<IFinancialFlowSource, CommitmentFlowSource>();
        services.AddScoped<IFinancialFlowSource, VariableSpendingFlowSource>();
        services.AddScoped<IFinancialFlowSource, GoalContributionFlowSource>();
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

        services.AddSingleton<IIntentEmbeddingMatcher, OnnxIntentEmbeddingMatcher>();
        services.AddSingleton<IParameterFuzzyMatcher, LevenshteinParameterFuzzyMatcher>();

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
