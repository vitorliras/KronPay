using Application.Abstractions.Common;
using Application.Abstractions.Pluggy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("pluggy")]
    public class PluggyController : ControllerBase
    {
        private readonly IPluggyService _pluggyService;
        private readonly ICurrentUserService _currentUser;


        public PluggyController(IPluggyService pluggyService, ICurrentUserService currentUser)
        {
            _pluggyService = pluggyService;
            _currentUser = currentUser;
        }

        //[AllowAnonymous]
        [HttpGet("connectors")]
        public async Task<IActionResult> GetConnectors()
        {
            var result = await _pluggyService.GetConnectorsAsync();

            return Ok(result);
        }

        [HttpGet("connect-token")]
        public async Task<IActionResult> CreateConnectToken()
        {
            var userId = _currentUser.UserId.ToString();

            var token = await _pluggyService
                .CreateConnectTokenAsync(userId);

            return Ok(token);
        }
    }
}
