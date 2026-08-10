namespace AuthService.Infrastructure.Exceptions;


public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 500)
        : base(message) => StatusCode = statusCode;
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, 404) { }
}


public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(message, 401) { }
}


public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409) { }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message, 403) { }
}