using AuthService.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace AuthService.Infrastructure.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}