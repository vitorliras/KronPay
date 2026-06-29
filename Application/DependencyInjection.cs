using Application.Executors;
using Application.Pipelines;
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

        // Auto-registro por convenção (ver ADR 0007): toda classe concreta
        // terminada em "UseCase" é registrada como Scoped automaticamente.
        var useCaseTypes = typeof(DependencyInjection).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("UseCase"));

        foreach (var type in useCaseTypes)
            services.AddScoped(type);

        return services;
    }
}
