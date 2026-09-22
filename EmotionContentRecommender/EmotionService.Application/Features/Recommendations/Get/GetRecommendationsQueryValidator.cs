using FluentValidation;

namespace EmotionService.Application.Features.Recommendations.Get;

public sealed class GetRecommendationsQueryValidator
    : AbstractValidator<GetRecommendationsQuery>
{
    public GetRecommendationsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("شناسهٔ کاربر نامعتبر است.");

        RuleFor(x => x.ItemTypeId)
            .GreaterThan(0)
            .WithMessage("نوع محتوا الزامی است.");

        RuleFor(x => x.PrimaryMoodId)
            .GreaterThan(0)
            .WithMessage("حالت احساسی اصلی الزامی است.");

        RuleFor(x => x.AdditionalMoodIds)
            .Must(HaveUniqueValues)
            .WithMessage("هر حالت احساسی تکمیلی فقط یک‌بار قابل انتخاب است.")
            .Must((query, values) => !values.Contains(query.PrimaryMoodId))
            .WithMessage("حالت احساسی اصلی نباید در انتخاب‌های تکمیلی تکرار شود.")
            .Must(values => values.Count <= 2)
            .WithMessage("حداکثر دو حالت احساسی تکمیلی قابل انتخاب است.");

        RuleForEach(x => x.AdditionalMoodIds)
            .GreaterThan(0)
            .WithMessage("شناسهٔ حالت احساسی تکمیلی نامعتبر است.");

        RuleFor(x => x.ThemeIds)
            .Must(HaveUniqueValues)
            .WithMessage("هر تم فقط یک‌بار قابل انتخاب است.")
            .Must(values => values.Count <= 5)
            .WithMessage("حداکثر پنج تم قابل انتخاب است.");

        RuleForEach(x => x.ThemeIds)
            .GreaterThan(0)
            .WithMessage("شناسهٔ تم نامعتبر است.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("تعداد نتایج هر صفحه باید بین ۱ تا ۵۰ باشد.");

        RuleFor(x => x.Cursor)
            .MaximumLength(2048)
            .When(x => x.Cursor is not null)
            .WithMessage("نشانگر صفحه‌بندی نامعتبر است.");
    }

    private static bool HaveUniqueValues(
        IReadOnlyCollection<int> values)
        => values.Distinct().Count() == values.Count;
}
