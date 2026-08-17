namespace AuthService.Infrastructure.Sms;

public class KavenegarSettings
{
    public const string SectionName = "Kavenegar";

    public string ApiKey    { get; init; } = string.Empty;
    public string Sender    { get; init; } = string.Empty;
    public string Template  { get; init; } = "verify";
}
