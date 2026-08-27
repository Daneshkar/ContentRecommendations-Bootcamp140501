using FluentValidation;

namespace EmotionService.Application.Features.Moods.Update;

public sealed class UpdateMoodCommandValidator
    : AbstractValidator<UpdateMoodCommand>
{
    public UpdateMoodCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}