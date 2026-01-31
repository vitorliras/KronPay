using Api.Contracts.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(
        this Shared.Results.Result result,
        IStringLocalizer localizer)
    {
        if (result.IsSuccess)
            return new OkResult();

        var error = new ApiErrorResponse
        {
            Code = result.Error!.Code,
            Message = localizer[result.Error.Code]
        };

        return new BadRequestObjectResult(error);
    }

    public static IActionResult ToActionResult<T>(
        this Shared.Results.ResultT<T> result,
        IStringLocalizer localizer)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        var error = new ApiErrorResponse
        {
            Code = result.ErrorCode,
            Message = localizer[result.ErrorMessage]
        };

        return new BadRequestObjectResult(error);
    }
}
