using System.Security.Claims;
using EmotionService.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Http;

namespace EmotionService.Infrastructure.Extensions;

public interface ICurrentUserService
{
    long GetUserId();
}

public sealed class CurrentUserService(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    public long GetUserId()
    {
        var user = httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ?? user?.FindFirstValue("sub");

        if (!long.TryParse(userIdClaim, out var userId)
            || userId <= 0)
        {
            throw new UnauthorizedException(
                "احراز هویت با شکست مواجه شد.");
        }

        return userId;
    }
}
