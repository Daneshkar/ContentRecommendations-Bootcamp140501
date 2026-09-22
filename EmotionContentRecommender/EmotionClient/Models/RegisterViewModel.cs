using System.ComponentModel.DataAnnotations;

namespace EmotionClient.Models;

public class RegisterViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters.")]
    [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "Use only letters, numbers, and underscores.")]
    public string   Username        { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    public string   Password        { get; set; } = string.Empty;

    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    [Required(ErrorMessage = "Confirm your password.")]
    public string   ConfirmPassword { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string?  Email           { get; set; }

    [RegularExpression("^09[0-9]{9}$", ErrorMessage = "Enter an Iranian mobile number in the format 09xxxxxxxxx.")]
    public string?  Mobile          { get; set; }

    [StringLength(100, ErrorMessage = "First name must be 100 characters or fewer.")]
    public string?  FirstName       { get; set; }

    [StringLength(100, ErrorMessage = "Last name must be 100 characters or fewer.")]
    public string?  LastName        { get; set; }
    public DateOnly? BirthDay       { get; set; }

    [Range(1, 3, ErrorMessage = "Choose a valid gender option.")]
    public byte?    Gender          { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(Mobile))
        {
            yield return new ValidationResult(
                "Enter an email address or a mobile number.",
                [nameof(Email), nameof(Mobile)]);
        }

        if (BirthDay.HasValue && BirthDay.Value > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            yield return new ValidationResult(
                "Date of birth cannot be in the future.",
                [nameof(BirthDay)]);
        }
    }
}
