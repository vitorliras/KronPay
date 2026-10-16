using Domain.Exceptions;
using Microsoft.Extensions.Localization;
using Shared.Localization;
using Shared.Resources;
using Shared.Results;
using System.Net;

namespace Api.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        IStringLocalizerFactory factory, IStringLocalizer<Messages> localizer,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            await HandleExceptionAsync(
                context,
                ex.Message,
                HttpStatusCode.BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(
                context,
                _localizer[MessageKeys.UnexpectedError],
                HttpStatusCode.InternalServerError
            );
        }
    }

    private async Task HandleExceptionAsync(
       HttpContext context,
       string errorMessage,
       HttpStatusCode statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var result = ResultEntity<bool>.Failure(
            code: statusCode.ToString(),
           message: errorMessage
       );

        var json = System.Text.Json.JsonSerializer.Serialize(result);
        await context.Response.WriteAsync(json);
    }
}
