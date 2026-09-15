using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace EmotionService.Infrastructure.Exceptions;

public sealed class ErrorResponse
{
    public bool    Success   { get; init; } = false;
    public int     Status    { get; init; }
    public string  Type      { get; init; } = string.Empty;
    public string  Message   { get; init; } = string.Empty;
    public string? ErrorCode { get; init; }
    public string  TraceId   { get; init; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }

    public static ErrorResponse From(
        int status, string type, string message, string traceId, string? errorCode = null)
        => new()
        {
            Status    = status,
            Type      = type,
            Message   = message,
            TraceId   = traceId,
            ErrorCode = errorCode
        };

    public static ErrorResponse Validation(
        IReadOnlyDictionary<string, string[]> errors,
        string traceId)
        => new()
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "ValidationError",
            Message = "یک یا چند مقدار ورودی نامعتبر است.",
            TraceId = traceId,
            ErrorCode = "VALIDATION_ERROR",
            Errors = errors
        };
}
