using AuthService.Application.Common;
using FluentValidation;
using MediatR;

namespace AuthService.Application.Features.Auth.Login;

public record LoginCommand(string Username, string Password)
    : IRequest<ApiResult<LoginResponse>>;

public record LoginResponse(
    long   UserId,
    string Username,
    string Role);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("نام کاربری الزامی است.")
            .MaximumLength(100).WithMessage("نام کاربری حداکثر ۱۰۰ کاراکتر می‌تواند باشد.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("رمز عبور الزامی است.")
            .MinimumLength(6).WithMessage("رمز عبور باید حداقل ۶ کاراکتر باشد.");
    }
}
