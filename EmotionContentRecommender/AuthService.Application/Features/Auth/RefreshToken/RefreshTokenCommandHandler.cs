using AuthService.Application.Common;
using AuthService.Infrastructure.Exceptions;
using AuthService.Infrastructure.Jwt;
using AuthService.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using DomainEntities = AuthService.Domain.Entities;

namespace AuthService.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, ApiResult<RefreshTokenResponse>>
{
    private readonly ApplicationDbContext _db;
    private readonly IJwtService _jwt;
    private readonly IHttpContextAccessor _httpContext;

    public RefreshTokenCommandHandler(
        ApplicationDbContext db,
        IJwtService jwt,
        IHttpContextAccessor httpContext)
    {
        _db = db;
        _jwt = jwt;
        _httpContext = httpContext;
    }

    public async Task<ApiResult<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken ct)
    {
        var storedToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == request.RefreshToken, ct);

        if (storedToken is null || !storedToken.IsActive)
            throw new UnauthorizedException("Refresh token نامعتبر یا منقضی شده است.");

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == storedToken.UserId, ct);

        if (user is null || !user.IsActive())
            throw new UnauthorizedException("حساب کاربری معتبر نیست.");

        storedToken.Revoke();

        var newAccessToken = _jwt.GenerateAccessToken(user);
        var newRawRefresh = _jwt.GenerateRefreshToken();
        var newRefreshToken = DomainEntities.RefreshToken.Create(user.Id, newRawRefresh, expirationDays: 7);

        _db.RefreshTokens.Add(newRefreshToken);
        await _db.SaveChangesAsync(ct);

        var response = _httpContext.HttpContext!.Response;

        response.Cookies.Append("access_token", newAccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        });

        response.Cookies.Append("refresh_token", newRawRefresh, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/api/auth/refresh"
        });

        return ApiResult<RefreshTokenResponse>.Success(
            new RefreshTokenResponse(user.Username, user.Role),
            "توکن با موفقیت تمدید شد.");
    }
}