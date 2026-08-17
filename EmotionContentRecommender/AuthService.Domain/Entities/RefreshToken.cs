using AuthService.Domain.Base;

namespace AuthService.Domain.Entities;

public class RefreshToken : AggregateRoot, IAuditable
{
    public long     UserId    { get; private set; }
    public string   Token     { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public bool     IsRevoked { get; private set; }

    //  IAuditable 
    public DateTime  CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private RefreshToken() { }

    public static RefreshToken Create(long userId, string token, int expirationDays)
        => new()
        {
            UserId    = userId,
            Token     = token,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsActive  => !IsRevoked && !IsExpired;

    public void Revoke()
    {
        IsRevoked = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
