namespace AuthService.Domain.Entities;

public class User
{
    public long      Id           { get; private set; }
    public string    Username     { get; private set; } = default!;
    public string?   FirstName    { get; private set; }
    public string?   LastName     { get; private set; }
    public string?   Email        { get; private set; }
    public string?   Mobile       { get; private set; }
    public string    PasswordHash { get; private set; } = default!;
    public bool      VerifyEmail  { get; private set; }
    public bool      VerifyMobile { get; private set; }
    public string?   Avatar       { get; private set; }
    public DateOnly? BirthDay     { get; private set; }
    public byte?     Gender       { get; private set; }
    public string    Role         { get; private set; } = "User";
    public DateTime  CreatedAt    { get; private set; }

    private User() { }

    public static User Create(
        string   username,
        string   passwordHash,
        string?  email     = null,
        string?  mobile    = null,
        string?  firstName = null,
        string?  lastName  = null,
        DateOnly? birthDay = null,
        byte?    gender    = null)
        => new()
        {
            Username     = username,
            PasswordHash = passwordHash,
            Email        = email,
            Mobile       = mobile,
            FirstName    = firstName,
            LastName     = lastName,
            BirthDay     = birthDay,
            Gender       = gender,
            CreatedAt    = DateTime.UtcNow
        };

    public void VerifyUserEmail()              => VerifyEmail  = true;
    public void VerifyUserMobile()             => VerifyMobile = true;
    public void ChangePassword(string newHash) => PasswordHash = newHash;
    public void UpdateAvatar(string url)       => Avatar       = url;
    public void MakeAdmin()                    => Role         = "Admin";
}
