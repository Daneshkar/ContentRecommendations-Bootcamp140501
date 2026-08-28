namespace EmotionService.Application.Features.Themes.ChangeStatus;

public sealed record ChangeThemeStatusResponse(
    int Id,
    string Name,
    bool IsActive
);