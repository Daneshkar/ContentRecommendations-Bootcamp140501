using MediatR;

namespace EmotionService.Application.Features.Themes.ChangeStatus;

public sealed record ChangeThemeStatusCommand(
    int Id,
    bool IsActive
) : IRequest<ChangeThemeStatusResponse>;