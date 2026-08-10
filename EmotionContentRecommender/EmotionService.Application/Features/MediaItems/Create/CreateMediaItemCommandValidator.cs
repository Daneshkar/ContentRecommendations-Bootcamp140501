using FluentValidation;

namespace EmotionService.Application.Features.MediaItems.Create;

public sealed class CreateMediaItemCommandValidator
    : AbstractValidator<CreateMediaItemCommand>
{
    public CreateMediaItemCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.CoverUrl)
            .MaximumLength(500);

        RuleFor(x => x.ItemTypeId)
            .NotEmpty()
            .GreaterThan(0);
    }
}