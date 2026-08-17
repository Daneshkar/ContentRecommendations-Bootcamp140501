namespace EmotionClient.Models;

public class RegisterViewModel
{
    public string   Username        { get; set; } = string.Empty;
    public string   Password        { get; set; } = string.Empty;
    public string   ConfirmPassword { get; set; } = string.Empty;
    public string?  Email           { get; set; }
    public string?  Mobile          { get; set; }
    public string?  FirstName       { get; set; }
    public string?  LastName        { get; set; }
    public DateOnly? BirthDay       { get; set; }
    public byte?    Gender          { get; set; }
}
