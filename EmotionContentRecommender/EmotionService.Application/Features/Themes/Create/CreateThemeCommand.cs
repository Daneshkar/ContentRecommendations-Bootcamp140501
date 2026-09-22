using MediatR;

namespace EmotionService.Application.Features.Themes.Create;

public sealed record CreateThemeCommand(
    string Name,
    string? Description
) : IRequest<CreateThemeResponse>;