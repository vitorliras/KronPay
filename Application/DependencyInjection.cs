using Application.Executors;
using Application.Pipelines;
using Application.Services.Assistant;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<UseCaseExecutor>();
        services.AddScoped(typeof(ValidationPipeline<,>));

        services.AddScoped<AssistantTree>();
        services.AddScoped<UserDataRichnessChecker>();
        services.AddScoped<CategoryParameterResolver>();
        services.AddScoped<TopicDisambiguationResolver>();
        services.AddScoped<GoalAssistantResolver>();
        services.AddScoped<CardAssistantResolver>();
        services.AddScoped<NotificationAssistantResolver>();

        var applicationAssembly = typeof(DependencyInjection).Assembly;

        var useCaseTypes = applicationAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("UseCase"));

        foreach (var type in useCaseTypes)
            services.AddScoped(type);

        var validatorTypes = applicationAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract);

        foreach (var validatorType in validatorTypes)
        {
            var validatorInterface = validatorType
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(IValidator<>));

            if (validatorInterface is not null)
                services.AddScoped(validatorInterface, validatorType);
        }

        return services;
    }
}
