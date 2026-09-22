using FluentValidation;

namespace EmotionService.Application.Features.Themes.ChangeStatus;

public sealed class ChangeThemeStatusCommandValidator
    : AbstractValidator<ChangeThemeStatusCommand>
{
    public ChangeThemeStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}