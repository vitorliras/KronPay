using System.Security.Claims;
using Application.Abstractions.Common;
using Microsoft.AspNetCore.Http;

namespace Infra.Security;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public int UserId
    {
        get
        {
            var sub = _httpContextAccessor.HttpContext?
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier)
                ?? _httpContextAccessor.HttpContext?
                .User
                .FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(sub))
                throw new UnauthorizedAccessException("User not authenticated");

            return int.Parse(sub);
        }
    }
}
