using System.Security.Claims;
using AuthService.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Http;

namespace AuthService.Infrastructure.Extensions;

public interface ICurrentUserService
{
    long   GetUserId();
    long?  GetCurrentUserId();
    string GetUsername();
    string GetRole();
    bool   IsAuthenticated();
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public long GetUserId()
    {
        var value = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User?.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(value))
            throw new UnauthorizedException("احراز هویت با شکست مواجه شد.");

        return long.Parse(value);
    }

    public long? GetCurrentUserId()
    {
        var value = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User?.FindFirstValue("sub");

        return long.TryParse(value, out var id) ? id : null;
    }

    public string GetUsername()
        => User?.FindFirstValue(ClaimTypes.Name)
        ?? User?.FindFirstValue("name")
        ?? string.Empty;

    public string GetRole()
        => User?.FindFirstValue(ClaimTypes.Role)
        ?? User?.FindFirstValue("role")
        ?? string.Empty;

    public bool IsAuthenticated()
        => User?.Identity?.IsAuthenticated ?? false;
}
