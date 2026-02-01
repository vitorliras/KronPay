using Api.Contracts.Errors;
using Domain.Exceptions;
using Microsoft.Extensions.Localization;
using Shared.Resources;
using System.Net;

namespace Api.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IStringLocalizer<Messages>  _localizer;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        IStringLocalizerFactory factory, IStringLocalizer<Messages> localizer)
    {
        _next = next;
        _localizer =  localizer;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            await HandleException(
                context,
                ex.Message,
                HttpStatusCode.BadRequest
            );
        }
        catch (Exception)
        {
            await HandleException(
                context,
                "UNEXPECTED_ERROR",
                HttpStatusCode.InternalServerError
            );
        }
    }

    private async Task HandleException(
        HttpContext context,
        string errorCode,
        HttpStatusCode statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ApiErrorResponse
        {
            Code = errorCode,
            Message = _localizer[errorCode]
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
