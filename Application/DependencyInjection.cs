using Application.Executors;
using Application.Pipelines;
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

        var applicationAssembly = typeof(DependencyInjection).Assembly;

        // Auto-registro por convenção (ver ADR 0007): toda classe concreta
        // terminada em "UseCase" é registrada como Scoped automaticamente.
        var useCaseTypes = applicationAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("UseCase"));

        foreach (var type in useCaseTypes)
            services.AddScoped(type);

        // Validators do módulo Card: auto-registro RESTRITO ao namespace
        // "Application.Validators.Card" — não ativa validators de outros módulos
        // (ver inconsistência #15 sobre o registro global ainda pendente).
        var cardValidators = applicationAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract
                && t.Namespace is not null
                && t.Namespace.StartsWith("Application.Validators.Card"));

        foreach (var validatorType in cardValidators)
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
