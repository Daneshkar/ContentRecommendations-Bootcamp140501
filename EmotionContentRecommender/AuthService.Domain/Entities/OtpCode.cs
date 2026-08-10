namespace AuthService.Domain.Entities;

public class OtpCode
{
    public long     Id        { get; private set; }
    public string   Mobile    { get; private set; } = default!;
    public string   Code      { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public bool     IsUsed    { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private OtpCode() { }

    public static OtpCode Create(string mobile, string code, int expirationMinutes = 2)
    {
        var now = DateTime.UtcNow;
        return new OtpCode
        {
            Mobile    = mobile.Trim(),
            Code      = code,
            ExpiresAt = now.AddMinutes(expirationMinutes),
            IsUsed    = false,
            CreatedAt = now
        };
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;

    public void MarkAsUsed()
    {
        IsUsed = true;
    }
}
