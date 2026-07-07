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

        // Auto-registro por tipo (ver ADR 0018): toda classe concreta que implementa
        // IValidator<TRequest> é registrada, em qualquer módulo/namespace — não depende
        // mais de casar prefixo de namespace, então nenhum validator fica "mudo" por
        // esquecimento de registro.
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
