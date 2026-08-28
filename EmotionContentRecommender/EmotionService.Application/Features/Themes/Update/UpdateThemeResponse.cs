namespace EmotionService.Application.Features.Themes.Update;

public sealed record UpdateThemeResponse(
    int Id,
    string Name,
    string? Description,
    bool IsActive
);