using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;
using Shared.Localization;
using Shared.Resources;
using Shared.Results;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace Api.Extensions;

public static class RateLimitingExtensions
{
    public const string AuthPolicy = "auth";

    private const int AuthPermitLimit = 10;
    private static readonly TimeSpan AuthWindow = TimeSpan.FromMinutes(1);

    public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(AuthPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = AuthPermitLimit,
                        Window = AuthWindow,
                        QueueLimit = 0
                    }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                var localizer = context.HttpContext.RequestServices
                    .GetRequiredService<IStringLocalizer<Messages>>();

                var result = ResultEntity<bool>.Failure(
                    message: localizer[MessageKeys.TooManyRequests],
                    code: MessageKeys.TooManyRequests);

                context.HttpContext.Response.Headers.RetryAfter = ((int)AuthWindow.TotalSeconds).ToString();
                context.HttpContext.Response.ContentType = "application/json";

                await context.HttpContext.Response.WriteAsync(
                    JsonSerializer.Serialize(result, JsonSerializerOptions.Web),
                    cancellationToken);
            };
        });

        return services;
    }
}
