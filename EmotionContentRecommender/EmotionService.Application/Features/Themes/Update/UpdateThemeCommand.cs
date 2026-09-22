using MediatR;

namespace EmotionService.Application.Features.Themes.Update;

public sealed record UpdateThemeCommand(
    int Id,
    string Name,
    string? Description
) : IRequest<UpdateThemeResponse>;