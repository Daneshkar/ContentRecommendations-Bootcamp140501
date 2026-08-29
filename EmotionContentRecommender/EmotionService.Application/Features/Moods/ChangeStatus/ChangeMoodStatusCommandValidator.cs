using FluentValidation;

namespace EmotionService.Application.Features.Moods.ChangeStatus;

public sealed class ChangeMoodStatusCommandValidator
    : AbstractValidator<ChangeMoodStatusCommand>
{
    public ChangeMoodStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}