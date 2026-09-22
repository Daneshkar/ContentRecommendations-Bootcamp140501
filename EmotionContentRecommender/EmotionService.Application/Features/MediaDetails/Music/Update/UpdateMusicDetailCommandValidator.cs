using FluentValidation;
namespace EmotionService.Application.Features.MediaDetails.Music.Update;
public sealed class UpdateMusicDetailCommandValidator : AbstractValidator<UpdateMusicDetailCommand>
{ 
    public UpdateMusicDetailCommandValidator() 
    { 
        RuleFor(x => x.MediaItemId).NotEmpty();
        RuleFor(x => x.Artist).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Album).MaximumLength(150); 
        RuleFor(x => x.Genre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DurationSeconds).InclusiveBetween(1, 86400);
        RuleFor(x => x.TrackNumber).GreaterThan(0).When(x => x.TrackNumber.HasValue);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Publisher).MaximumLength(150);
        RuleFor(x => x.Language).MaximumLength(50);
        RuleFor(x => x.ReleaseYear).InclusiveBetween(1800, 2100).When(x => x.ReleaseYear.HasValue);
    }
}
