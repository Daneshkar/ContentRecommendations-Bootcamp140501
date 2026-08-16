using FluentValidation;

namespace EmotionService.Application.Features.Genres.Create;

public sealed class CreateGenreCommandValidator
    : AbstractValidator<CreateGenreCommand>
{
    public CreateGenreCommandValidator()
    {
        RuleFor(x => x.ItemTypeId)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}