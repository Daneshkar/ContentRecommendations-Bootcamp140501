using FluentValidation;
namespace EmotionService.Application.Features.MediaDetails.Book.Create;
public sealed class CreateBookDetailCommandValidator : AbstractValidator<CreateBookDetailCommand>
{
    public CreateBookDetailCommandValidator()
    {
        RuleFor(x => x.MediaItemId).NotEmpty();
        RuleFor(x => x.Author).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Publisher).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Genre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ISBN).NotEmpty().MaximumLength(20);
        RuleFor(x => x.PageCount).GreaterThan(0).When(x => x.PageCount.HasValue);
        RuleFor(x => x.Language).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(3000);
        RuleFor(x => x.Edition).MaximumLength(100);
        RuleFor(x => x.PublicationDate)
            .Must(x => !x.HasValue || x.Value <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Publication date cannot be in the future.");
    }
}
