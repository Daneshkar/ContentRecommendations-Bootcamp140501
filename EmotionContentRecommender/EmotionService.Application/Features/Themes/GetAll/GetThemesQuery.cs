using MediatR;

namespace EmotionService.Application.Features.Themes.GetAll;

public sealed record GetThemesQuery(
    bool? IsActive
) : IRequest<IReadOnlyList<ThemeListItemResponse>>;