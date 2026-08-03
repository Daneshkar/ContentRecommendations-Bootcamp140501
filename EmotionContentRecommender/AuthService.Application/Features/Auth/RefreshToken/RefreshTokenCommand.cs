using AuthService.Application.Common;
using FluentValidation;
using MediatR;

namespace AuthService.Application.Features.Auth.RefreshToken;

public record RefreshTokenCommand(string RefreshToken)
    : IRequest<ApiResult<RefreshTokenResponse>>;

public record RefreshTokenResponse(string Username, string Role);

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token الزامی است.");
    }
}
