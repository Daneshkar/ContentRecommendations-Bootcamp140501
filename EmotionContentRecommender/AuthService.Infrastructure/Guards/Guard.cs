using AuthService.Infrastructure.Exceptions;

namespace AuthService.Infrastructure.Guards;

public static class Guard
{
    public static void AgainstNull(object? value, string message)
    {
        if (value is null)
            throw new AppException(message, 400);
    }

    public static void AgainstNullOrEmpty(string? value, string message)
    {
        if (string.IsNullOrEmpty(value))
            throw new AppException(message, 400);
    }

    public static void AgainstNullOrWhiteSpace(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new AppException(message, 400);
    }

    public static void AgainstFalse(bool condition, string message)
    {
        if (!condition)
            throw new BusinessException(message);
    }

    public static void AgainstNotFound<T>(T? value, string message) where T : class
    {
        if (value is null)
            throw new NotFoundException(message);
    }

    public static void AgainstUnauthorized(bool condition, string message = "احراز هویت با شکست مواجه شد.")
    {
        if (!condition)
            throw new UnauthorizedException(message);
    }
}