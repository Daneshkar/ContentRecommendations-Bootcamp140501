using FluentValidation;

namespace EmotionService.Application.Features.Experiences.Update;

public sealed class UpdateExperienceCommandValidator
    : AbstractValidator<UpdateExperienceCommand>
{
    public UpdateExperienceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .GreaterThan(0);

        RuleFor(x => x.Score)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.MoodIds)
            .NotEmpty()
            .Must(HaveUniqueValues)
            .WithMessage("هر حالت احساسی فقط یک‌بار قابل انتخاب است.");

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
