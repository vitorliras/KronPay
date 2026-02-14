using Api.Contracts.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(
        this Shared.Results.ResultEntity<T> result,
        IStringLocalizer localizer)
    {
        if (result.Message != null)
            result.Message = localizer[result.Message];

        if (result.IsSuccess) {
            
            return new OkObjectResult(result);
        }

        return new BadRequestObjectResult(result);
    }
}
