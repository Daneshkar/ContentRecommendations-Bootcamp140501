using FluentValidation;

namespace EmotionService.Application.Features.Genres.Update;

public sealed class UpdateGenreCommandValidator
    : AbstractValidator<UpdateGenreCommand>
{
    public UpdateGenreCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.ItemTypeId)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}