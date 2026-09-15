using FluentValidation;

namespace EmotionService.Application.Features.Experiences.Create;

public sealed class CreateExperienceCommandValidator
    : AbstractValidator<CreateExperienceCommand>
{
    public CreateExperienceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("شناسهٔ کاربر نامعتبر است.");

        RuleFor(x => x.MediaItemId)
            .NotEmpty()
            .WithMessage("شناسهٔ محتوا الزامی است.");

        RuleFor(x => x.Score)
            .InclusiveBetween(1, 5)
            .WithMessage("امتیاز باید بین ۱ تا ۵ باشد.");

        RuleFor(x => x.MoodIds)
            .NotEmpty()
            .WithMessage("حداقل یک حالت احساسی باید انتخاب شود.")
            .Must(HaveUniqueValues)
            .WithMessage("هر حالت احساسات فقط یک‌بار قابل انتخاب است.");

        RuleForEach(x => x.MoodIds)
            .GreaterThan(0)
            .WithMessage("شناسهٔ حالت احساسی باید بزرگ‌تر از صفر باشد.");

        RuleFor(x => x.ThemeIds)
            .NotEmpty()
            .WithMessage("حداقل یک تم باید انتخاب شود.")
            .Must(HaveUniqueValues)
            .WithMessage("هر تم فقط یک‌بار قابل انتخاب است.");

        RuleForEach(x => x.ThemeIds)
            .GreaterThan(0)
            .WithMessage("شناسهٔ تم باید بزرگ‌تر از صفر باشد.");
    }

    private static bool HaveUniqueValues(
        IReadOnlyCollection<int> values)
        => values.Distinct().Count() == values.Count;
}
