using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

public sealed class AccessHeaderMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public AccessHeaderMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requiredKey = _config["Security:AccessHeaderKey"];
        var headerName = "X-Access-Key";

        if (string.IsNullOrWhiteSpace(requiredKey))
        {
            await _next(context);
            return;
        }

        var endpoint = context.GetEndpoint();
        var allowAnonymous = endpoint?.Metadata
            .GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null;

        if (allowAnonymous)
        {
            if (!context.Request.Headers.TryGetValue(headerName, out var value) ||
                value != requiredKey)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Missing or invalid access header.");
                return;
            }
        }

        await _next(context);
    }
}
