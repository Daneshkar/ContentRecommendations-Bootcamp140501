namespace EmotionService.Application.Features.Moods.GetAll;

public sealed record MoodListItemResponse(
    int Id,
    string Name,
    string? Description,
    bool IsActive
);