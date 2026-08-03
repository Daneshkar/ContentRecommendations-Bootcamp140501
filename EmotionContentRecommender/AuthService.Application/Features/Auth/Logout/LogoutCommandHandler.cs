using AuthService.Application.Common;
using AuthService.Infrastructure.Extensions;
using AuthService.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Auth.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ApiResult>
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService  _currentUser;
    private readonly IHttpContextAccessor _httpContext;

    public LogoutCommandHandler(
        ApplicationDbContext db,
        ICurrentUserService  currentUser,
        IHttpContextAccessor httpContext)
    {
        _db          = db;
        _currentUser = currentUser;
        _httpContext = httpContext;
    }

    public async Task<ApiResult> Handle(LogoutCommand request, CancellationToken ct)
    {
        var userId = _currentUser.GetUserId();

        await _db.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ExecuteUpdateAsync(
                s => s.SetProperty(r => r.IsRevoked, true), ct);

        RemoveAuthCookies();

        return ApiResult.Success("خروج با موفقیت انجام شد.");
    }

    private void RemoveAuthCookies()
    {
        var response = _httpContext.HttpContext!.Response;

        response.Cookies.Append("access_token", string.Empty, new CookieOptions
        {
            HttpOnly = true,
            Secure   = true,
            SameSite = SameSiteMode.Lax,
            Expires  = DateTimeOffset.UtcNow.AddDays(-1)
        });

        response.Cookies.Append("refresh_token", string.Empty, new CookieOptions
        {
            HttpOnly = true,
            Secure   = true,
            SameSite = SameSiteMode.Lax,
            Expires  = DateTimeOffset.UtcNow.AddDays(-1),
            Path     = "/api/auth/refresh"
        });
    }
}
