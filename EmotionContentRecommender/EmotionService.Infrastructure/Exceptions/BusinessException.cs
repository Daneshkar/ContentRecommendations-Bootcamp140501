namespace EmotionService.Infrastructure.Exceptions;

public class BusinessException : Exception
{
    public string ErrorCode { get; }

    public BusinessException(string message, string errorCode = "BUSINESS_RULE_VIOLATION")
        : base(message) => ErrorCode = errorCode;
}
