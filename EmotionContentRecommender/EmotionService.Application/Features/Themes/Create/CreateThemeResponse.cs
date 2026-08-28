namespace EmotionService.Application.Features.Themes.Create;

public sealed record CreateThemeResponse(
    int Id,
    string Name,
    string? Description,
    bool IsActive
);