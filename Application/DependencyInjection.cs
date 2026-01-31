using Application.Executors;
using Application.Pipelines;
using Application.UseCases.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<UseCaseExecutor>();
        services.AddScoped<CreateUserUseCase>();
        services.AddScoped(typeof(ValidationPipeline<,>));

        return services;
    }
}
