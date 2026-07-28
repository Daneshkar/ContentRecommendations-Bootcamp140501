namespace AuthService.Domain.Entities;

public class RefreshToken
{
    public long     Id        { get; private set; }
    public long     UserId    { get; private set; }
    public string   Token     { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public bool     IsRevoked { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private RefreshToken() { }

    public static RefreshToken Create(long userId, string token, DateTime expiresAt)
        => new()
        {
            UserId    = userId,
            Token     = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

    public void Revoke() => IsRevoked = true;

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsActive  => !IsRevoked && !IsExpired;
}
