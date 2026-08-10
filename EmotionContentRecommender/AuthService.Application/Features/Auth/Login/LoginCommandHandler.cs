using AuthService.Application.Common;
using AuthService.Infrastructure.Exceptions;
using AuthService.Infrastructure.Guards;
using AuthService.Infrastructure.Jwt;
using AuthService.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using DomainEntities = AuthService.Domain.Entities;

namespace AuthService.Application.Features.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResult<LoginResponse>>
{
    private readonly ApplicationDbContext _db;
    private readonly IJwtService _jwt;
    private readonly IHttpContextAccessor _httpContext;

    public LoginCommandHandler(
        ApplicationDbContext db,
        IJwtService jwt,
        IHttpContextAccessor httpContext)
    {
        _db = db;
        _jwt = jwt;
        _httpContext = httpContext;
    }

    public async Task<ApiResult<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken ct)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Username == request.Username.Trim().ToLower(), ct);

        Guard.AgainstNotFound(user, "نام کاربری یا رمز عبور اشتباه است.");

        if (!user!.IsActive())
            throw new BusinessException("حساب کاربری شما غیرفعال است.", "ACCOUNT_INACTIVE");

        Guard.AgainstUnauthorized(
            BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash),
            "نام کاربری یا رمز عبور اشتباه است.");

        await _db.RefreshTokens
            .Where(r => r.UserId == user.Id && !r.IsRevoked)
            .ExecuteUpdateAsync(
                s => s.SetProperty(r => r.IsRevoked, true), ct);

        var accessToken = _jwt.GenerateAccessToken(user);
        var rawRefresh = _jwt.GenerateRefreshToken();
        var refreshToken = DomainEntities.RefreshToken.Create(user.Id, rawRefresh, expirationDays: 7);

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync(ct);

        SetAuthCookies(accessToken, rawRefresh);

        return ApiResult<LoginResponse>.Success(
            new LoginResponse(user.Id, user.Username, user.Role.ToString()),
            "ورود موفقیت‌آمیز بود.");
    }

    private void SetAuthCookies(string accessToken, string refreshToken)
    {
        var response = _httpContext.HttpContext!.Response;

        response.Cookies.Append("access_token", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        });

        response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/api/auth/refresh"
        });
    }
}