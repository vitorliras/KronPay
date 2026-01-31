using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Resources;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/resources")]
public sealed class ResourcesController : ControllerBase
{
    private readonly IStringLocalizer<Messages> _localizer;

    public ResourcesController(IStringLocalizer<Messages> localizer)
    {
        _localizer = localizer;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var messages = _localizer
            .GetAllStrings()
            .ToDictionary(
                x => x.Name,
                x => x.Value
            );

        return Ok(messages);
    }
}
