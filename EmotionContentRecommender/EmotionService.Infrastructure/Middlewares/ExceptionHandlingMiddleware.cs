using System.Text.Json;
using EmotionService.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace EmotionService.Infrastructure.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate                      _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(
        RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        ErrorResponse response;

        switch (exception)
        {
            case BusinessException bex:
                _logger.LogWarning(
                    "BusinessException | TraceId: {TraceId} | Code: {Code} | {Message}",
                    traceId, bex.ErrorCode, bex.Message);
                response = ErrorResponse.From(422, "BusinessError", bex.Message, traceId, bex.ErrorCode);
                break;

            case AppException aex:
                _logger.LogWarning(
                    "AppException | TraceId: {TraceId} | Status: {Status} | {Message}",
                    traceId, aex.StatusCode, aex.Message);
                response = ErrorResponse.From(aex.StatusCode, "ApplicationError", aex.Message, traceId);
                break;

            case SqlException sex:
                _logger.LogError(sex,
                    "SqlException | TraceId: {TraceId} | Number: {Number}",
                    traceId, sex.Number);

                var sqlMessage = sex.Number switch
                {
                    2627 or 2601 => "داده تکراری است و قابل ثبت نمی‌باشد.",
                    547           => "عملیات به دلیل وابستگی داده‌ها امکان‌پذیر نیست.",
                    4060          => "دسترسی به پایگاه داده ممکن نیست.",
                    _             => "خطای پایگاه داده رخ داده است."
                };
                response = ErrorResponse.From(500, "DatabaseError", sqlMessage, traceId, $"SQL_{sex.Number}");
                break;

            default:
                _logger.LogError(exception,
                    "UnhandledException | TraceId: {TraceId} | {Message}",
                    traceId, exception.Message);
                response = ErrorResponse.From(500, "InternalServerError", "خطای داخلی سرور رخ داده است.", traceId);
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = response.Status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, _jsonOptions));
    }
}
