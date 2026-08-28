namespace EmotionService.Application.Features.Themes.GetAll;

public sealed record ThemeListItemResponse(
    int Id,
    string Name,
    string? Description,
    bool IsActive
);