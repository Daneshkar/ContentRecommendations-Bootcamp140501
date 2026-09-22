namespace EmotionService.Contracts.Themes;

public sealed record CreateThemeRequest(
    string Name,
    string? Description
);