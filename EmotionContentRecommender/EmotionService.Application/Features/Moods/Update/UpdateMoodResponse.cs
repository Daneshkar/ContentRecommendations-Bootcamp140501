namespace EmotionService.Application.Features.Moods.Update;

public sealed record UpdateMoodResponse(
    int Id,
    string Name,
    string? Description,
    bool IsActive
);