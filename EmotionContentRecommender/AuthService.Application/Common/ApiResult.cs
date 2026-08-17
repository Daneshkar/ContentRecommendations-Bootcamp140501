namespace AuthService.Application.Common;

public class ApiResult<T>
{
    public bool    IsSuccess  { get; private set; }
    public T?      Data       { get; private set; }
    public string? Message    { get; private set; }
    public int     StatusCode { get; private set; }

    private ApiResult() { }

    public static ApiResult<T> Success(T data, string? message = null)
        => new() { IsSuccess = true,  Data = data,    Message = message, StatusCode = 200 };

    public static ApiResult<T> Failure(string message, int statusCode = 400)
        => new() { IsSuccess = false, Message = message, StatusCode = statusCode };

    public static ApiResult<T> NotFound(string message)
        => new() { IsSuccess = false, Message = message, StatusCode = 404 };
}

public class ApiResult
{
    public bool    IsSuccess  { get; private set; }
    public string? Message    { get; private set; }
    public int     StatusCode { get; private set; }

    private ApiResult() { }

    public static ApiResult Success(string? message = null)
        => new() { IsSuccess = true,  Message = message, StatusCode = 200 };

    public static ApiResult Failure(string message, int statusCode = 400)
        => new() { IsSuccess = false, Message = message, StatusCode = statusCode };
}
