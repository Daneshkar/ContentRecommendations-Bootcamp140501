namespace EmotionService.Contracts.Themes;

public sealed record UpdateThemeRequest(
    string Name,
    string? Description
);