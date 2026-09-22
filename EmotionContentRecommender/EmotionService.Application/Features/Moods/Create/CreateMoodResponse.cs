namespace EmotionService.Application.Features.Moods.Create;

public sealed record CreateMoodResponse(
    int Id,
    string Name,
    string? Description,
    bool IsActive
);