using System.Net;
using System.Text.Json;
using AuthService.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
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
            // ─── خطاهای منطق تجاری ──────────────────────────────────────────
            case BusinessException bex:
                _logger.LogWarning(
                    "BusinessException | TraceId: {TraceId} | Code: {Code} | {Message}",
                    traceId, bex.ErrorCode, bex.Message);

                response = ErrorResponse.From(
                    status: 422,
                    type: "BusinessError",
                    message: bex.Message,
                    traceId: traceId,
                    errorCode: bex.ErrorCode);
                break;

            // ─── خطاهای HTTP سفارشی (AppException و زیرکلاس‌هایش) ──────────
            case AppException aex:
                _logger.LogWarning(
                    "AppException | TraceId: {TraceId} | Status: {Status} | {Message}",
                    traceId, aex.StatusCode, aex.Message);

                response = ErrorResponse.From(
                    status: aex.StatusCode,
                    type: "ApplicationError",
                    message: aex.Message,
                    traceId: traceId);
                break;

            // ─── خطاهای دیتابیس SQL ─────────────────────────────────────────
            case SqlException sex:
                _logger.LogError(sex,
                    "SqlException | TraceId: {TraceId} | SqlNumber: {Number}",
                    traceId, sex.Number);

                var sqlMessage = sex.Number switch
                {
                    2627 or 2601 => "داده تکراری است و قابل ثبت نمی‌باشد.",
                    547 => "عملیات به دلیل وابستگی داده‌ها امکان‌پذیر نیست.",
                    4060 => "دسترسی به پایگاه داده ممکن نیست.",
                    _ => "خطای پایگاه داده رخ داده است."
                };

                response = ErrorResponse.From(
                    status: 500,
                    type: "DatabaseError",
                    message: sqlMessage,
                    traceId: traceId,
                    errorCode: $"SQL_{sex.Number}");
                break;

            // ─── خطاهای پیش‌بینی نشده ───────────────────────────────────────
            default:
                _logger.LogError(exception,
                    "UnhandledException | TraceId: {TraceId} | {Message}",
                    traceId, exception.Message);

                response = ErrorResponse.From(
                    status: 500,
                    type: "InternalServerError",
                    message: "خطای داخلی سرور رخ داده است.",
                    traceId: traceId);
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.Status;

        var json = JsonSerializer.Serialize(response, _jsonOptions);
        await context.Response.WriteAsync(json);
    }
}