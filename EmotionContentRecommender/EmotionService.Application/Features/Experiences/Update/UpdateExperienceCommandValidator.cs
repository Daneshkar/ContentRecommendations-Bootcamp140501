using FluentValidation;

namespace EmotionService.Application.Features.Experiences.Update;

public sealed class UpdateExperienceCommandValidator
    : AbstractValidator<UpdateExperienceCommand>
{
    public UpdateExperienceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("شناسهٔ تجربه الزامی است.");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("شناسهٔ کاربر نامعتبر است.");

        RuleFor(x => x.Score)
            .InclusiveBetween(1, 5)
            .WithMessage("امتیاز باید بین ۱ تا ۵ باشد.");

        RuleFor(x => x.MoodIds)
            .NotEmpty()
            .WithMessage("حداقل یک حالت احساسی باید انتخاب شود.")
            .Must(HaveUniqueValues)
            .WithMessage("هر حالت احساسی فقط یک‌بار قابل انتخاب است.");

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
