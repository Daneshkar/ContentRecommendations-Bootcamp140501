using EmotionService.Infrastructure.BackgroundJobs;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Jwt;
using EmotionService.Infrastructure.Middlewares;
using EmotionService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

namespace EmotionService.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(
                    typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddOptions<AggregateWeightJobOptions>()
            .Bind(configuration.GetSection(
                AggregateWeightJobOptions.SectionName))
            .Validate(
                options => options.Hour is >= 0 and <= 23,
                "AggregateWeightJob:Hour must be between 0 and 23.")
            .Validate(
                options => options.Minute is >= 0 and <= 59,
                "AggregateWeightJob:Minute must be between 0 and 59.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.TimeZoneId),
                "AggregateWeightJob:TimeZoneId is required.")
            .Validate(
                options => options.BatchSize is >= 1 and <= 10_000,
                "AggregateWeightJob:BatchSize must be between 1 and 10000.")
            .Validate(
                options => options.MaxBatchesPerRun is >= 1 and <= 1_000,
                "AggregateWeightJob:MaxBatchesPerRun must be between 1 and 1000.")
            .ValidateOnStart();

        services.AddScoped<AggregateWeightQueue>();
        services.AddScoped<AggregateWeightProcessor>();
        services.AddHostedService<AggregateWeightBackgroundService>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtSettings.SectionName);

        services.Configure<JwtSettings>(jwtSection);

        var jwtSettings = jwtSection.Get<JwtSettings>()!;

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(
                                    jwtSettings.SecretKey)),

                        ClockSkew = TimeSpan.Zero,

                        RoleClaimType =
                            System.Security.Claims.ClaimTypes.Role,

                        NameClaimType =
                            System.Security.Claims.ClaimTypes.Name
                    };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        var logger = ctx.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtAuthentication");

                        logger.LogError(
                            ctx.Exception,
                            "JWT authentication failed. ExceptionType: {ExceptionType}",
                            ctx.Exception.GetType().Name);

                        return Task.CompletedTask;
                    },

                    OnMessageReceived = ctx =>
                    {
                        if (ctx.Request.Cookies.TryGetValue(
                                "access_token",
                                out var token))
                        {
                            ctx.Token = token;
                        }

                        return Task.CompletedTask;
                    },

                    OnChallenge = async ctx =>
                    {
                        ctx.HandleResponse();

                        ctx.Response.StatusCode =
                            StatusCodes.Status401Unauthorized;

                        ctx.Response.ContentType =
                            "application/json";

                        var response = ErrorResponse.From(
                            status: 401,
                            type: "AuthenticationError",
                            message: "احراز هویت با شکست مواجه شد.",
                            traceId:
                                ctx.HttpContext.TraceIdentifier);

                        await ctx.Response.WriteAsync(
                            JsonSerializer.Serialize(
                                response,
                                new JsonSerializerOptions
                                {
                                    PropertyNamingPolicy =
                                        JsonNamingPolicy.CamelCase
                                }));
                    },

                    OnForbidden = async ctx =>
                    {
                        ctx.Response.StatusCode =
                            StatusCodes.Status403Forbidden;

                        ctx.Response.ContentType =
                            "application/json";

                        var response = ErrorResponse.From(
                            status: 403,
                            type: "AuthorizationError",
                            message:
                                "شما دسترسی لازم برای این عملیات را ندارید.",
                            traceId:
                                ctx.HttpContext.TraceIdentifier);

                        await ctx.Response.WriteAsync(
                            JsonSerializer.Serialize(
                                response,
                                new JsonSerializerOptions
                                {
                                    PropertyNamingPolicy =
                                        JsonNamingPolicy.CamelCase
                                }));
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static IApplicationBuilder UseExceptionHandling(
        this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
