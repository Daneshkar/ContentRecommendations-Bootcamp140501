using FluentValidation;

namespace EmotionService.Application.Features.Recommendations.GetByExperience;

public sealed class GetExperienceRecommendationsQueryValidator
    : AbstractValidator<GetExperienceRecommendationsQuery>
{
    public GetExperienceRecommendationsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("شناسهٔ کاربر نامعتبر است.");

        RuleFor(x => x.ExperienceId)
            .NotEmpty()
            .WithMessage("شناسهٔ تجربه الزامی است.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("تعداد نتایج هر صفحه باید بین ۱ تا ۵۰ باشد.");

        RuleFor(x => x.Cursor)
            .MaximumLength(2048)
            .When(x => x.Cursor is not null)
            .WithMessage("نشانگر صفحه‌بندی نامعتبر است.");
    }
}
