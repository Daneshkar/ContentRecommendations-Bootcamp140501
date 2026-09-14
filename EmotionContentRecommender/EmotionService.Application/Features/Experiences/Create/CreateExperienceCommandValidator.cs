using FluentValidation;

namespace EmotionService.Application.Features.Experiences.Create;

public sealed class CreateExperienceCommandValidator
    : AbstractValidator<CreateExperienceCommand>
{
    public CreateExperienceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0);

        RuleFor(x => x.MediaItemId)
            .NotEmpty();

        RuleFor(x => x.Score)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.MoodIds)
            .NotEmpty()
            .Must(HaveUniqueValues)
            .WithMessage("هر حالت احساسات فقط یک‌بار قابل انتخاب است.");

        RuleForEach(x => x.MoodIds)
            .GreaterThan(0);

        RuleFor(x => x.ThemeIds)
            .NotEmpty()
            .Must(HaveUniqueValues)
            .WithMessage("هر تم فقط یک‌بار قابل انتخاب است.");

        RuleForEach(x => x.ThemeIds)
            .GreaterThan(0);
    }

    private static bool HaveUniqueValues(
        IReadOnlyCollection<int> values)
        => values.Distinct().Count() == values.Count;
}
