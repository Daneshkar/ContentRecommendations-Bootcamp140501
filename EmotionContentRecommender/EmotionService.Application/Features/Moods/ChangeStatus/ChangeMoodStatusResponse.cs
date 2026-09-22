namespace EmotionService.Application.Features.Moods.ChangeStatus;

public sealed record ChangeMoodStatusResponse(
    int Id,
    string Name,
    bool IsActive
);