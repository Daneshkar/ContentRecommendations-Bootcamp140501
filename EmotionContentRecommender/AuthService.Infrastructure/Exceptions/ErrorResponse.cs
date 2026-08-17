namespace AuthService.Infrastructure.Exceptions;

public sealed class ErrorResponse
{
    public bool Success { get; init; } = false;
    public int Status { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? ErrorCode { get; init; }
    public string TraceId { get; init; } = string.Empty;

    public static ErrorResponse From(int status, string type, string message,
        string traceId, string? errorCode = null) => new()
        {
            Status = status,
            Type = type,
            Message = message,
            TraceId = traceId,
            ErrorCode = errorCode
        };
}