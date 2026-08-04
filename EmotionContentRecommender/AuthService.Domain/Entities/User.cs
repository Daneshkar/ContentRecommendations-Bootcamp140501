using AuthService.Domain.Base;
using AuthService.Domain.Events;
using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public class User : AggregateRoot, IAuditable
{
    public string     Username     { get; private set; } = default!;
    public string?    FirstName    { get; private set; }
    public string?    LastName     { get; private set; }
    public string?    Email        { get; private set; }
    public string?    Mobile       { get; private set; }
    public string     PasswordHash { get; private set; } = default!;
    public bool       VerifyEmail  { get; private set; }
    public bool       VerifyMobile { get; private set; }
    public string?    AvatarUser   { get; private set; }
    public DateOnly?  BirthDay     { get; private set; }
    public GenderType? Gender      { get; private set; }
    public UserRole   Role         { get; private set; } = UserRole.User;
    public UserStatus Status       { get; private set; } = UserStatus.Active;

    //  IAuditable 
    public DateTime  CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private User() { }

    //  Factory Method 
    public static User Create(
        string     username,
        string     passwordHash,
        string?    email     = null,
        string?    mobile    = null,
        string?    firstName = null,
        string?    lastName  = null,
        DateOnly?  birthDay  = null,
        byte?      gender    = null)
    {
        var user = new User
        {
            Username     = username.Trim().ToLower(),
            PasswordHash = passwordHash,
            Email        = email?.Trim().ToLower(),
            Mobile       = mobile?.Trim(),
            FirstName    = firstName?.Trim(),
            LastName     = lastName?.Trim(),
            BirthDay     = birthDay,
            Gender       = gender.HasValue ? (GenderType)gender.Value : null,
            Status       = UserStatus.Active,
            CreatedAt    = DateTime.UtcNow
        };

        user.AddDomainEvent(new UserCreatedEvent(user.Username));
        return user;
    }

    
    public bool IsActive()   => Status == UserStatus.Active;
    public bool IsInactive() => Status == UserStatus.Inactive;
    public bool IsBanned()   => Status == UserStatus.Banned;

    public void VerifyUserEmail()
    {
        VerifyEmail = true;
        UpdatedAt   = DateTime.UtcNow;
    }

    public void VerifyUserMobile()
    {
        VerifyMobile = true;
        UpdatedAt    = DateTime.UtcNow;
    }

    public void ChangePassword(string newHash)
    {
        PasswordHash = newHash;
        UpdatedAt    = DateTime.UtcNow;
    }

    public void UpdateAvatar(string url)
    {
        AvatarUser    = url;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MakeAdmin()
    {
        Role      = UserRole.Admin;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status    = UserStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Ban()
    {
        Status    = UserStatus.Banned;
        UpdatedAt = DateTime.UtcNow;
    }
}
