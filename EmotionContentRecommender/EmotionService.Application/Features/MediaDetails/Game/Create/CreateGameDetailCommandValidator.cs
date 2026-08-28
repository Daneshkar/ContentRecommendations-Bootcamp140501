using FluentValidation;
namespace EmotionService.Application.Features.MediaDetails.Game.Create;
public sealed class CreateGameDetailCommandValidator : AbstractValidator<CreateGameDetailCommand>
{
    public CreateGameDetailCommandValidator()
    {
        RuleFor(x => x.MediaItemId).NotEmpty();
        RuleFor(x => x.Developer).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Publisher).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Genre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Platform).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(3000);
        RuleFor(x => x.ReleaseYear).InclusiveBetween(1970, 2100).When(x => x.ReleaseYear.HasValue);
        RuleFor(x => x.AgeRating).MaximumLength(30);
        RuleFor(x => x.GameMode).MaximumLength(100);
        RuleFor(x => x.Engine).MaximumLength(100);
    }
}
