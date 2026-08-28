using FluentValidation;

namespace EmotionService.Application.Features.Themes.Create;

public sealed class CreateThemeCommandValidator
    : AbstractValidator<CreateThemeCommand>
{
    public CreateThemeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}