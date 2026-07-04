using Application.DTOs.Users;
using Application.Executors;
using Application.Pipelines;
using Application.Validators.Users;
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

        // Validators auto-registrados de forma RESTRITA aos módulos Card, Planning e Auth
        // (namespaces "Application.Validators.Card"/".Planning"/".Auth") — não ativa
        // validators de outros módulos (ver inconsistência #15 sobre o registro global
        // ainda pendente; SPEC 0015 vai generalizar esse scan por tipo para todos os
        // módulos).
        var moduleValidators = applicationAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract
                && t.Namespace is not null
                && (t.Namespace.StartsWith("Application.Validators.Card")
                    || t.Namespace.StartsWith("Application.Validators.Planning")
                    || t.Namespace.StartsWith("Application.Validators.Auth")));

        foreach (var validatorType in moduleValidators)
        {
            var validatorInterface = validatorType
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(IValidator<>));

            if (validatorInterface is not null)
                services.AddScoped(validatorInterface, validatorType);
        }

        // Registro pontual (não via scan): "Application.Validators.Users" não está no
        // scan automático acima porque esse namespace já tem um validator pré-existente
        // (CreateUserValidator) que nunca rodou em produção (inconsistência #15);
        // ampliar o scan ativaria esse validator dormente como efeito colateral não
        // planejado desta tarefa. Registrando só o validator novo, pontualmente, até que
        // a SPEC 0015 avalie e generalize o scan para todos os módulos.
        services.AddScoped<IValidator<UploadProfilePhotoRequest>, UploadProfilePhotoRequestValidator>();

        return services;
    }
}
