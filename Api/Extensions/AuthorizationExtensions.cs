using Microsoft.AspNetCore.Authorization;

namespace Api.Extensions;

public static class AuthorizationExtensions
{
    public const string PublicAssetPolicy = "public-asset";

    public static IServiceCollection AddApiAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy(PublicAssetPolicy, policy => policy.RequireAssertion(_ => true));
        });

        return services;
    }
}
