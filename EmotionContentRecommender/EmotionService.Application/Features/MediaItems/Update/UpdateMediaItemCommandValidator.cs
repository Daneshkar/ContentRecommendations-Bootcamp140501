using FluentValidation;

namespace EmotionService.Application.Features.MediaItems.Update;

public sealed class UpdateMediaItemCommandValidator
    : AbstractValidator<UpdateMediaItemCommand>
{
    public UpdateMediaItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.ItemTypeId)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.CoverUrl)
            .MaximumLength(500);
    }
}