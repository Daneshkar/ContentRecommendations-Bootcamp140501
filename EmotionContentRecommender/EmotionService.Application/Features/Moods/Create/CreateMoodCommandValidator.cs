using FluentValidation;

namespace EmotionService.Application.Features.Moods.Create;

public sealed class CreateMoodCommandValidator
    : AbstractValidator<CreateMoodCommand>
{
    public CreateMoodCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}