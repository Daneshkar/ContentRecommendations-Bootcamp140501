namespace EmotionService.Application.Features.Themes.GetById;

public sealed record GetThemeByIdResponse(
    int Id,
    string Name,
    string? Description,
    bool IsActive
);