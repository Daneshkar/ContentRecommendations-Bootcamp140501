using AuthService.Application.Common;
using FluentValidation;
using MediatR;

namespace AuthService.Application.Features.Auth.Register;

public record RegisterCommand(
    string    Username,
    string    Password,
    string    ConfirmPassword,
    string?   Email,
    string?   Mobile,
    string?   FirstName,
    string?   LastName,
    DateOnly? BirthDay,
    byte?     Gender)
    : IRequest<ApiResult<RegisterResponse>>;

public record RegisterResponse(long UserId, string Username);

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("نام کاربری الزامی است.")
            .MinimumLength(3).WithMessage("نام کاربری باید حداقل ۳ کاراکتر باشد.")
            .MaximumLength(100).WithMessage("نام کاربری حداکثر ۱۰۰ کاراکتر می‌تواند باشد.")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("نام کاربری فقط می‌تواند شامل حروف، اعداد و _ باشد.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("رمز عبور الزامی است.")
            .MinimumLength(6).WithMessage("رمز عبور باید حداقل ۶ کاراکتر باشد.")
            .MaximumLength(100);

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("رمز عبور و تکرار آن باید یکسان باشند.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("فرمت ایمیل صحیح نیست.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Mobile)
            .Matches(@"^09[0-9]{9}$").WithMessage("فرمت شماره موبایل صحیح نیست.")
            .When(x => !string.IsNullOrEmpty(x.Mobile));

        RuleFor(x => x.Gender)
            .InclusiveBetween((byte)1, (byte)3).WithMessage("مقدار جنسیت معتبر نیست.")
            .When(x => x.Gender.HasValue);

        RuleFor(x => x.BirthDay)
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("تاریخ تولد نمی‌تواند در آینده باشد.")
            .When(x => x.BirthDay.HasValue);

        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.Email) || !string.IsNullOrEmpty(x.Mobile))
            .WithMessage("حداقل یکی از ایمیل یا شماره موبایل باید وارد شود.");
    }
}
