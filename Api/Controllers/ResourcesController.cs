using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Resources;

namespace Api.Controllers;

[Authorize]
[ApiController]
[AllowAnonymous]
[Route("api/resources")]
public sealed class ResourcesController : ControllerBase
{
    private readonly IStringLocalizer<Messages> _localizer;

    public ResourcesController(IStringLocalizer<Messages> localizer)
    {
        _localizer = localizer;
    }

    [HttpGet("[action]")]
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

    [HttpGet("[action]")]
    public IActionResult GetByName(string name)
    {
        var message = _localizer
            .GetAllStrings().Where(n => n.Name == name)
            .ToDictionary(
                x => x.Name,
                x => x.Value
            );

        return Ok(message);
    }

    [HttpGet("[action]")]
    public IActionResult GetAllByName(IEnumerable<string> names)
    {
        var messages = new Dictionary<string, string>();
        var all = _localizer.GetAllStrings().ToDictionary(x => x.Name, x => x.Value);

        foreach (var name in names)
        {
            if (all.TryGetValue(name, out var value))
                messages[name] = value;
        }

        return Ok(messages);
    }
}
