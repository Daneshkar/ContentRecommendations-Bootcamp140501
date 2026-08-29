using FluentValidation;
namespace EmotionService.Application.Features.MediaDetails.Movie.Update;
public sealed class UpdateMovieDetailCommandValidator : AbstractValidator<UpdateMovieDetailCommand>
{
    public UpdateMovieDetailCommandValidator()
    {
        RuleFor(x => x.MediaItemId).NotEmpty();
        RuleFor(x => x.Director).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Genre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Synopsis).NotEmpty().MaximumLength(3000);
        RuleFor(x => x.DurationMinutes).InclusiveBetween(1, 1000);
        RuleFor(x => x.ReleaseYear).InclusiveBetween(1888, 2100).When(x => x.ReleaseYear.HasValue);
        RuleFor(x => x.Language).MaximumLength(50);
        RuleFor(x => x.Country).MaximumLength(100);
        RuleFor(x => x.AgeRating).MaximumLength(30);
        RuleFor(x => x.Cast).MaximumLength(1000);
        RuleFor(x => x.Studio).MaximumLength(150);
    }
}
